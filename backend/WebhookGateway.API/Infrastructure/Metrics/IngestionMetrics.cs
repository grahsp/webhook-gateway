namespace WebhookGateway.API.Infrastructure.Metrics;

using System.Diagnostics.Metrics;

public sealed class IngestionMetrics
{
	public const string MeterName = "WebhookGateway.Ingestion";

	private readonly Counter<long> _webhooksReceived;
	private readonly Histogram<double> _ingestionDuration;

	public IngestionMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(MeterName);

		_webhooksReceived = meter.CreateCounter<long>("webhook_ingestion_total_requests");
		_ingestionDuration = meter.CreateHistogram<double>("webhook_ingestion_duration_ms", unit: "ms");
	}

	public void WebhookReceived(string source, string eventType)
		=> _webhooksReceived.Add(1,
			new KeyValuePair<string, object?>("source", source),
			new KeyValuePair<string, object?>("event_type", eventType));

	public void RecordIngestionDuration(double ms)
		=> _ingestionDuration.Record(ms);
}