using Microsoft.Extensions.Options;

namespace WebhookGateway.API.Infrastructure.Messaging;

public sealed class RabbitMqInitializer(
	IRabbitMqConnectionProvider provider,
	IOptions<RabbitMqOptions> options)
	: IRabbitMqInitializer
{
	private readonly RabbitMqOptions _options = options.Value;
	
	public async Task InitializeAsync(CancellationToken ct = default)
	{
		var connection = await provider.GetConnectionAsync();
		await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
		
		await channel.QueueDeclareAsync(
			queue: _options.DeliveryQueue,
			durable: true,
			exclusive: false,
			autoDelete: false,
			cancellationToken: ct);
		
		await channel.QueueDeclareAsync(
			queue: _options.RetryQueue,
			durable: true,
			exclusive: false,
			autoDelete: false,
			arguments: new Dictionary<string, object?>
			{
				["x-message-ttl"] = (int)_options.RetryDelaySeconds.TotalMilliseconds,
				["x-dead-letter-exchange"] = string.Empty,
				["x-dead-letter-routing-key"] = _options.DeliveryQueue
			},
			cancellationToken: ct);
	}
}