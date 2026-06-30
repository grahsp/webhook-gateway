using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Infrastructure.Health;

public sealed class PostgresHealthCheck(AppDbContext db) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var canConnect = await db.Database.CanConnectAsync(cancellationToken);

			return canConnect
				? HealthCheckResult.Healthy("Postgres is reachable.")
				: HealthCheckResult.Unhealthy("Postgres is not reachable.");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("Postgres health check failed.", ex);
		}
	}
}
