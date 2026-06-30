using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebhookGateway.API.Infrastructure.Messaging;

namespace WebhookGateway.API.Infrastructure.Health;

public sealed class RabbitMqHealthCheck(IRabbitMqConnectionProvider provider) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var connection = await provider.GetConnectionAsync();

			return connection.IsOpen
				? HealthCheckResult.Healthy("RabbitMQ connection is open.")
				: HealthCheckResult.Unhealthy("RabbitMQ connection is closed.");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("RabbitMQ health check failed.", ex);
		}
	}
}
