using System.Diagnostics.Metrics;

namespace WebhookGateway.API.Infrastructure.Metrics;

public sealed class WorkerMetrics
{
	public const string MeterName = "WebhookGateway.Worker";
	
	private readonly Counter<long> _messagesReceived;
	private readonly Counter<long> _messagesCompleted;
	private readonly Counter<long> _messageRetried;
	private readonly Counter<long> _messagesFailed;
	private readonly Histogram<double> _processingDuration;

	public WorkerMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(MeterName);
		
		_messagesReceived = meter.CreateCounter<long>(
			name: "worker_messages_received",
			description: "Number of messages received from RabbitMQ.");
		
		_messagesCompleted = meter.CreateCounter<long>(
			name: "worker_message_completed",
			description: "Number of successfully processed messages.");
		
		_messageRetried = meter.CreateCounter<long>(
			name: "worker_message_retried",
			description: "Number of failed message deliveries that were retried.");
		
		_messagesFailed = meter.CreateCounter<long>(
			name: "worker_message_failed",
			description: "Number of failed message deliveries.");

		_processingDuration = meter.CreateHistogram<double>(
			name: "worker_message_processing_duration_ms",
			unit: "ms",
			description: "Time spent processing a message.");
	}

	public void MessageReceived()
		=> _messagesReceived.Add(1);
	
	public void MessageCompleted()
		=> _messagesCompleted.Add(1);
	
	public void MessageRetried()
		=> _messageRetried.Add(1);
	
	public void MessageFailed(int count = 1)
		=> _messagesFailed.Add(count);
	
	public void RecordProcessingDuration(double ms)
		=> _processingDuration.Record(ms);
}