namespace WebhookGateway.API.Infrastructure.Messaging;

public interface IMessagePublisher
{
	Task PublishAsync<T>(T message, CancellationToken ct = default);
}