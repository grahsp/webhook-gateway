using Microsoft.Extensions.Options;

namespace WebhookGateway.API.Infrastructure.Messaging;

public sealed class RabbitMqInitializer(IRabbitMqConnectionProvider provider, IOptions<RabbitMqOptions> options) : IRabbitMqInitializer
{
	private readonly RabbitMqOptions _options = options.Value;
	
	public async Task InitializeAsync(CancellationToken ct = default)
	{
		var connection = await provider.GetConnectionAsync();
		await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
		
		await channel.QueueDeclareAsync(
			queue: _options.QueueName,
			durable: true,
			exclusive: false,
			autoDelete: false,
			cancellationToken: ct);
	}
}