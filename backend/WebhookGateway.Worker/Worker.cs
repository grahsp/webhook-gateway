using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Infrastructure.Messaging;
using WebhookGateway.API.Infrastructure.Metrics;
using WebhookGateway.Worker.Logging;

namespace WebhookGateway.Worker;

public sealed class Worker(
	IMessagePublisher publisher,
	IRabbitMqConnectionProvider provider,
	IOptions<RabbitMqOptions> options,
	IServiceScopeFactory scopes,
	WorkerMetrics metrics,
	ILogger<Worker> logger)
	: BackgroundService
{
	private const int DeliveryIdErrorSampleSize = 10;
	private readonly RabbitMqOptions _options = options.Value;
	
	private const ushort PrefetchCount = 100;
	private const int BatchSize = 100;
	private static readonly TimeSpan BatchTimeout = TimeSpan.FromMilliseconds(200);

	private readonly Channel<Message<Guid>> _buffer = Channel.CreateBounded<Message<Guid>>(new BoundedChannelOptions(PrefetchCount)
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
				
				metrics.MessageReceived();
				await _buffer.Writer.WriteAsync(new Message<Guid>(deliveryId, ea.DeliveryTag), ct);
			}
			catch (Exception ex)
			{
				logger.RabbitMqMessageBufferFailed(ex, ea.DeliveryTag);

				await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: ct);
			}
		};

		await channel.BasicConsumeAsync(queue: _options.DeliveryQueue, autoAck: false, consumer: consumer, cancellationToken: ct);

		logger.WorkerConsumerStarted(_options.DeliveryQueue, PrefetchCount, BatchSize, BatchTimeout);

		while (!ct.IsCancellationRequested)
		{
			var batch = await ReadBatchAsync(ct);

			if (batch.Count == 0)
				continue;

			var start = Stopwatch.GetTimestamp();
			await ProcessBatchAsync(channel, batch, ct);
			
			var duration = Stopwatch.GetElapsedTime(start);
			metrics.RecordProcessingDuration(duration.TotalMilliseconds / batch.Count);
		}
	}

	private async Task<IReadOnlyList<Message<Guid>>> ReadBatchAsync(CancellationToken ct)
	{
		var batch = new List<Message<Guid>>(BatchSize);

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

	private async Task ProcessBatchAsync(IChannel channel, IReadOnlyList<Message<Guid>> batch, CancellationToken ct)
	{
		try
		{
			await using var scope = scopes.CreateAsyncScope();

			var dispatcher = scope.ServiceProvider
				.GetRequiredService<IWebhookBatchDeliveryDispatcher>();

			var results = await dispatcher
				.DispatchAsync(batch.Select(x => x.Body), ct);

			var lookup = results.ToDictionary(x => x.DeliveryId);

			foreach (var message in batch)
			{
				if (!lookup.TryGetValue(message.Body, out var result))
				{
					logger.DispatcherResultMissing(message.Body);
					throw new InvalidOperationException($"Dispatcher returned no result for delivery '{message.Body}'.");
				}

				await ApplyActionAsync(channel, message, result.Action, ct);
			}
		}
		catch (Exception ex)
		{
			logger.DeliveryBatchProcessingFailed(
				ex,
				batch.Count,
				string.Join(',', batch.Take(DeliveryIdErrorSampleSize).Select(x => x.Body)));
			logger.DeliveryBatchProcessingFailedTrace(
				batch.Count,
				string.Join(',', batch.Select(x => x.Body)));

			var highestTag = batch.Max(x => x.DeliveryTag);
			await channel.BasicNackAsync(highestTag, multiple: true, requeue: true, cancellationToken: ct);
		}
	}
	
	private async Task ApplyActionAsync(IChannel channel, Message<Guid> message, DeliveryAction action, CancellationToken ct)
	{
		switch (action)
		{
			case DeliveryAction.Ack:
				metrics.MessageCompleted();
				break;

			case DeliveryAction.Retry:
				metrics.MessageRetried();
				await publisher.PublishAsync(_options.RetryQueue, message.Body, ct);
				logger.DeliveryRepublishedForRetry(message.Body, _options.RetryQueue);
				break;

			case DeliveryAction.DeadLetter:
				metrics.MessageFailed();
				await publisher.PublishAsync(_options.DeadLetterQueue, message.Body, ct);
				logger.DeliveryPublishedToDeadLetter(message.Body, _options.DeadLetterQueue);
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(action));
			
		}
		
		await channel.BasicAckAsync(message.DeliveryTag, multiple: false, cancellationToken: ct);
	}
}
