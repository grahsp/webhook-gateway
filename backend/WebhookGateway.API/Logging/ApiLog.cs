using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Logging;

public static partial class ApiLog
{
	[LoggerMessage(EventId = 1000, EventName = nameof(RequestCanceled), Level = LogLevel.Information,
		Message = "Request was canceled: {RequestMethod} {RequestPath}")]
	public static partial void RequestCanceled(this ILogger logger, string requestMethod, PathString requestPath);

	[LoggerMessage(EventId = 1001, EventName = nameof(UnhandledRequestException), Level = LogLevel.Error,
		Message = "Unhandled exception while processing {RequestMethod} {RequestPath}")]
	public static partial void UnhandledRequestException(this ILogger logger, Exception exception, string requestMethod, PathString requestPath);

	[LoggerMessage(EventId = 1100, EventName = nameof(WebhookReceived), Level = LogLevel.Information,
		Message = "Webhook received for route {WebhookRouteId}: event {WebhookEventId}, delivery {DeliveryId}, source {Source}, type {EventType}, destinations {DestinationCount}")]
	public static partial void WebhookReceived(this ILogger logger, Guid webhookRouteId, Guid webhookEventId, string deliveryId, WebhookSource source, string eventType, int destinationCount);

	[LoggerMessage(EventId = 1101, EventName = nameof(DuplicateWebhookIgnored), Level = LogLevel.Information,
		Message = "Duplicate webhook ignored for route {WebhookRouteId}: delivery {DeliveryId}, source {Source}, type {EventType}")]
	public static partial void DuplicateWebhookIgnored(this ILogger logger, Guid webhookRouteId, string deliveryId, WebhookSource source, string eventType);

	[LoggerMessage(EventId = 1102, EventName = nameof(WebhookEnqueueFailed), Level = LogLevel.Error,
		Message = "Failed to enqueue delivery {WebhookDeliveryId} for webhook event {WebhookEventId} to queue {QueueName}")]
	public static partial void WebhookEnqueueFailed(this ILogger logger, Exception exception, Guid webhookEventId, Guid webhookDeliveryId, string queueName);

	[LoggerMessage(EventId = 1103, EventName = nameof(DeliveryQueued), Level = LogLevel.Information,
		Message = "Delivery queued: {WebhookDeliveryId} for webhook event {WebhookEventId} to queue {QueueName}")]
	public static partial void DeliveryQueued(this ILogger logger, Guid webhookEventId, Guid webhookDeliveryId, string queueName);

	[LoggerMessage(EventId = 1104, EventName = nameof(InvalidWebhookSignature), Level = LogLevel.Warning,
		Message = "Invalid webhook signature for source {Source}")]
	public static partial void InvalidWebhookSignature(this ILogger logger, WebhookSource source);

	[LoggerMessage(EventId = 1200, EventName = nameof(WebhookRouteCreated), Level = LogLevel.Debug,
		Message = "Webhook route created: {WebhookRouteId}, source {Source}")]
	public static partial void WebhookRouteCreated(this ILogger logger, Guid webhookRouteId, WebhookSource source);

	[LoggerMessage(EventId = 1201, EventName = nameof(WebhookRouteDeleted), Level = LogLevel.Debug,
		Message = "Webhook route deleted: {WebhookRouteId}")]
	public static partial void WebhookRouteDeleted(this ILogger logger, Guid webhookRouteId);

	[LoggerMessage(EventId = 1202, EventName = nameof(WebhookDestinationCreated), Level = LogLevel.Debug,
		Message = "Webhook destination created: {WebhookDestinationId} for route {WebhookRouteId}")]
	public static partial void WebhookDestinationCreated(this ILogger logger, Guid webhookRouteId, Guid webhookDestinationId);

	[LoggerMessage(EventId = 1203, EventName = nameof(WebhookDestinationDeleted), Level = LogLevel.Debug,
		Message = "Webhook destination deleted: {WebhookDestinationId} for route {WebhookRouteId}")]
	public static partial void WebhookDestinationDeleted(this ILogger logger, Guid webhookRouteId, Guid webhookDestinationId);
}
