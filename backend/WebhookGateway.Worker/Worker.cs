using System.Text.Json;
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
	protected async override Task ExecuteAsync(CancellationToken ct)
	{
		var connection = await provider.GetConnectionAsync();
		var channel = await connection.CreateChannelAsync(cancellationToken: ct);

		var consumer = new AsyncEventingBasicConsumer(channel);

		consumer.ReceivedAsync += async (_, ea) =>
		{
			var deliveryId = JsonSerializer.Deserialize<Guid>(ea.Body.Span);

			try
			{
				await using var scope = scopes.CreateAsyncScope();

				var dispatcher = scope.ServiceProvider
					.GetRequiredService<IWebhookDeliveryDispatcher>();
				
				await dispatcher.DispatchAsync(deliveryId);

				await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to process delivery {DeliveryId}", deliveryId);

				await channel.BasicNackAsync(
					ea.DeliveryTag,
					multiple: false,
					requeue: true,
					cancellationToken: ct);
			}
		};

		await channel.BasicConsumeAsync(
			queue: "webhook-deliveries",
			autoAck: false,
			consumer: consumer,
			cancellationToken: ct);

		await Task.Delay(Timeout.Infinite, ct);
	}
}