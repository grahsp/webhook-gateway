using System.Text.Json;
using System.Threading.Channels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Infrastructure.Messaging;

namespace WebhookGateway.Worker;

public sealed class Worker(
	IRabbitMqConnectionProvider provider,
	IServiceScopeFactory scopes,
	ILogger<Worker> logger)
	: BackgroundService
{
	private const ushort PrefetchCount = 100;
	private const int BatchSize = 100;
	private static readonly TimeSpan BatchTimeout = TimeSpan.FromMilliseconds(200);

	private readonly Channel<QueuedDelivery> _buffer = Channel.CreateBounded<QueuedDelivery>(new BoundedChannelOptions(PrefetchCount)
	{
		SingleReader = true,
		SingleWriter = false,
		FullMode = BoundedChannelFullMode.Wait
	});

	protected async override Task ExecuteAsync(CancellationToken ct)
	{
		var connection = await provider.GetConnectionAsync();
		await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

		await channel.BasicQosAsync(
			prefetchSize: 0,
			prefetchCount: PrefetchCount,
			global: false,
			cancellationToken: ct);

		var consumer = new AsyncEventingBasicConsumer(channel);

		consumer.ReceivedAsync += async (_, ea) =>
		{
			try
			{
				var deliveryId = JsonSerializer.Deserialize<Guid>(ea.Body.Span);
				await _buffer.Writer.WriteAsync(new QueuedDelivery(deliveryId, ea.DeliveryTag), ct);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to buffer RabbitMQ message");

				await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: ct);
			}
		};

		await channel.BasicConsumeAsync(queue: "webhook-deliveries", autoAck: false, consumer: consumer, cancellationToken: ct);

		logger.LogInformation("Worker started consuming messages");

		while (!ct.IsCancellationRequested)
		{
			var batch = await ReadBatchAsync(ct);

			if (batch.Count == 0)
				continue;

			await ProcessBatchAsync(channel, batch, ct);
		}
	}

	private async Task<IReadOnlyList<QueuedDelivery>> ReadBatchAsync(CancellationToken ct)
	{
		var batch = new List<QueuedDelivery>(BatchSize);

		var first = await _buffer.Reader.ReadAsync(ct);
		batch.Add(first);

		using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
		timeoutCts.CancelAfter(BatchTimeout);

		while (batch.Count < BatchSize)
		{
			try
			{
				var item = await _buffer.Reader.ReadAsync(timeoutCts.Token);
				batch.Add(item);
			}
			catch (OperationCanceledException) when (!ct.IsCancellationRequested)
			{
				break;
			}
		}

		return batch;
	}

	private async Task ProcessBatchAsync(IChannel channel, IReadOnlyList<QueuedDelivery> batch, CancellationToken ct)
	{
		try
		{
			await using var scope = scopes.CreateAsyncScope();

			var dispatcher = scope.ServiceProvider
				.GetRequiredService<IWebhookBatchDeliveryDispatcher>();

			var results = await dispatcher
				.DispatchAsync(batch.Select(x => x.DeliveryId), ct);

			var lookup = results.ToDictionary(x => x.DeliveryId);

			foreach (var message in batch)
			{
				if (!lookup.TryGetValue(message.DeliveryId, out var result))
					throw new InvalidOperationException($"Dispatcher returned no result for delivery '{message.DeliveryId}'.");

				await ApplyActionAsync(channel, message.DeliveryTag, result.Action, ct);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to process delivery batch");

			var highestTag = batch.Max(x => x.DeliveryTag);
			await channel.BasicNackAsync(highestTag, multiple: true, requeue: true, cancellationToken: ct);
		}
	}
	
	private static ValueTask ApplyActionAsync(IChannel channel, ulong deliveryTag, DeliveryAction action, CancellationToken ct)
	{
		return action switch
		{
			DeliveryAction.Ack =>
				channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: ct),

			DeliveryAction.Retry =>
				channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true, cancellationToken: ct),

			DeliveryAction.DeadLetter =>
				channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false, cancellationToken: ct),

			_ => throw new ArgumentOutOfRangeException(nameof(action))
		};
	}
}