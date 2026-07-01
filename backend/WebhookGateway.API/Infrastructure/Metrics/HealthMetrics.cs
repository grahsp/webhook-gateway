using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebhookGateway.API.Infrastructure.Metrics;

public sealed class HealthMetrics : IHealthCheckPublisher
{
	public const string MeterName = "WebhookGateway.Health";

	private readonly ConcurrentDictionary<string, int> _checks = [];
	private int _ready;

	public HealthMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(MeterName);

		meter.CreateObservableGauge(
			"webhook_health_ready",
			() => Volatile.Read(ref _ready),
			description: "Readiness status for this service. 1 is healthy, 0 is unhealthy.");

		meter.CreateObservableGauge(
			"webhook_health_check",
			ObserveChecks,
			description: "Health check status by dependency. 1 is healthy, 0 is unhealthy.");
	}

	public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
	{
		Volatile.Write(ref _ready, report.Status == HealthStatus.Healthy ? 1 : 0);

		foreach (var entry in report.Entries)
			_checks[entry.Key] = entry.Value.Status == HealthStatus.Healthy ? 1 : 0;

		return Task.CompletedTask;
	}

	private IEnumerable<Measurement<int>> ObserveChecks()
		=> _checks.Select(check => new Measurement<int>(
			check.Value,
			new KeyValuePair<string, object?>("check", check.Key)));
}
