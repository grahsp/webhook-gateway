namespace WebhookGateway.API.Infrastructure.Messaging;

public interface IRabbitMqInitializer
{
	Task InitializeAsync(CancellationToken ct = default);
}