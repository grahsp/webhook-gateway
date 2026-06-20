using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace WebhookGateway.API.Infrastructure.Messaging;

public sealed class RabbitMqConnectionProvider : IRabbitMqConnectionProvider
{
	private readonly Lazy<Task<IConnection>> _connection;

	public RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options)
	{
		_connection = new Lazy<Task<IConnection>>(() =>
		{
			var factory = new ConnectionFactory
			{
				HostName = options.Value.Host,
				UserName = options.Value.Username,
				Password = options.Value.Password
			};

			return factory.CreateConnectionAsync();
		});
	}

	public Task<IConnection> GetConnectionAsync()
		=> _connection.Value;
}