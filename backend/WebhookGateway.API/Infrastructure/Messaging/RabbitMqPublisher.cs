using System.Text.Json;
using RabbitMQ.Client;

namespace WebhookGateway.API.Infrastructure.Messaging;

public sealed class RabbitMqPublisher(IRabbitMqConnectionProvider provider) : IMessagePublisher
{
	public async Task PublishAsync<T>(T message, CancellationToken ct = default)
	{
		var connection = await provider.GetConnectionAsync();
		await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

		var body = JsonSerializer.SerializeToUtf8Bytes(message);

		await channel.BasicPublishAsync(
			exchange: "",
			routingKey: "webhook-deliveries",
			mandatory: true,
			body: body,
			cancellationToken: ct);
	}
}