using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Infrastructure.Webhooks;

namespace WebhookGateway.API.Logging;

public static partial class DeliveryLog
{
	[LoggerMessage(EventId = 4000, EventName = nameof(DeliveryDispatchException), Level = LogLevel.Error,
		Message = "Delivery dispatch failed with exception: {WebhookDeliveryId}, destination {WebhookDestinationId}, attempt {AttemptCount}")]
	public static partial void DeliveryDispatchException(this ILogger logger, Exception exception, Guid webhookDeliveryId, Guid webhookDestinationId, int attemptCount);

	[LoggerMessage(EventId = 4001, EventName = nameof(DeliveryRetryScheduled), Level = LogLevel.Information,
		Message = "Delivery retry scheduled: {WebhookDeliveryId}, destination {WebhookDestinationId}, status {StatusCode}, attempt {AttemptCount}")]
	public static partial void DeliveryRetryScheduled(this ILogger logger, Guid webhookDeliveryId, Guid webhookDestinationId, int? statusCode, int attemptCount);

	[LoggerMessage(EventId = 4002, EventName = nameof(DeliveryDeadLettered), Level = LogLevel.Warning,
		Message = "Delivery dead-lettered: {WebhookDeliveryId}, destination {WebhookDestinationId}, status {StatusCode}, attempt {AttemptCount}, result {ResultType}")]
	public static partial void DeliveryDeadLettered(this ILogger logger, Guid webhookDeliveryId, Guid webhookDestinationId, int? statusCode, int attemptCount, DeliveryResultType resultType);

	[LoggerMessage(EventId = 4003, EventName = nameof(DeliveryCompleted), Level = LogLevel.Trace,
		Message = "Delivery completed: {WebhookDeliveryId}, destination {WebhookDestinationId}, status {StatusCode}, attempt {AttemptCount}, action {Action}")]
	public static partial void DeliveryCompleted(this ILogger logger, Guid webhookDeliveryId, Guid webhookDestinationId, int? statusCode, int attemptCount, DeliveryAction action);

	[LoggerMessage(EventId = 4004, EventName = nameof(EndpointReturned5xxRetrying), Level = LogLevel.Warning,
		Message = "Endpoint returned 5xx; retrying delivery {WebhookDeliveryId}, destination {WebhookDestinationId}, status {StatusCode}, attempt {AttemptCount}")]
	public static partial void EndpointReturned5xxRetrying(this ILogger logger, Guid webhookDeliveryId, Guid webhookDestinationId, int statusCode, int attemptCount);
}
