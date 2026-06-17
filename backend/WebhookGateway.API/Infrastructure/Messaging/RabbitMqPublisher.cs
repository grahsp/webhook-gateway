using System.Text.Json;
using RabbitMQ.Client;

namespace WebhookGateway.API.Infrastructure.Messaging;

public sealed class RabbitMqPublisher(IConnection connection) : IMessagePublisher
{
	public async Task PublishAsync<T>(T message, CancellationToken ct = default)
	{
		await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

		await channel.QueueDeclareAsync(
			queue: "webhook-deliveries",
			durable: true,
			exclusive: false,
			autoDelete: false,
			arguments: null,
			cancellationToken: ct);

		var body = JsonSerializer.SerializeToUtf8Bytes(message);

		await channel.BasicPublishAsync(
			exchange: "",
			routingKey: "webhook-deliveries",
			mandatory: true,
			body: body,
			cancellationToken: ct);
	}
}