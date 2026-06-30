namespace WebhookGateway.Worker.Logging;

public static partial class WorkerLog
{
	[LoggerMessage(EventId = 3000, EventName = nameof(WorkerConsumerStarted), Level = LogLevel.Information,
		Message = "Worker started consuming messages from {QueueName} with prefetch {PrefetchCount}, batch size {BatchSize}, batch timeout {BatchTimeout}")]
	public static partial void WorkerConsumerStarted(this ILogger logger, string queueName, ushort prefetchCount, int batchSize, TimeSpan batchTimeout);

	[LoggerMessage(EventId = 3001, EventName = nameof(RabbitMqMessageBufferFailed), Level = LogLevel.Error,
		Message = "Failed to buffer RabbitMQ message with delivery tag {DeliveryTag}")]
	public static partial void RabbitMqMessageBufferFailed(this ILogger logger, Exception exception, ulong deliveryTag);

	[LoggerMessage(EventId = 3002, EventName = nameof(DeliveryBatchProcessingFailed), Level = LogLevel.Error,
		Message = "Failed to process delivery batch of size {BatchSize}. Delivery sample: {WebhookDeliveryIdSample}")]
	public static partial void DeliveryBatchProcessingFailed(this ILogger logger, Exception exception, int batchSize, string webhookDeliveryIdSample);

	[LoggerMessage(EventId = 3003, EventName = nameof(DispatcherResultMissing), Level = LogLevel.Error,
		Message = "Dispatcher returned no result for delivery {WebhookDeliveryId}")]
	public static partial void DispatcherResultMissing(this ILogger logger, Guid webhookDeliveryId);

	[LoggerMessage(EventId = 3004, EventName = nameof(DeliveryRepublishedForRetry), Level = LogLevel.Trace,
		Message = "Delivery republished for retry: {WebhookDeliveryId} to queue {QueueName}")]
	public static partial void DeliveryRepublishedForRetry(this ILogger logger, Guid webhookDeliveryId, string queueName);

	[LoggerMessage(EventId = 3005, EventName = nameof(DeliveryPublishedToDeadLetter), Level = LogLevel.Trace,
		Message = "Delivery published to dead-letter queue: {WebhookDeliveryId} to queue {QueueName}")]
	public static partial void DeliveryPublishedToDeadLetter(this ILogger logger, Guid webhookDeliveryId, string queueName);

	[LoggerMessage(EventId = 3006, EventName = nameof(DeliveryBatchProcessingFailedTrace), Level = LogLevel.Trace,
		Message = "Failed to process delivery batch of size {BatchSize}. Deliveries: {WebhookDeliveryIds}")]
	public static partial void DeliveryBatchProcessingFailedTrace(this ILogger logger, int batchSize, string webhookDeliveryIds);
}
