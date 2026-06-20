using RabbitMQ.Client;

namespace WebhookGateway.API.Infrastructure.Messaging;

public interface IRabbitMqConnectionProvider
{
	Task<IConnection> GetConnectionAsync();
}