namespace WebhookGateway.API.Logging;

public static partial class RabbitMqLog
{
	[LoggerMessage(EventId = 2000, EventName = nameof(RabbitMqInitializationStarted), Level = LogLevel.Information,
		Message = "RabbitMQ initialization started: delivery {DeliveryQueue}, retry {RetryQueue}, dead-letter {DeadLetterQueue}, retry delay {RetryDelay}")]
	public static partial void RabbitMqInitializationStarted(this ILogger logger, string deliveryQueue, string retryQueue, string deadLetterQueue, TimeSpan retryDelay);

	[LoggerMessage(EventId = 2001, EventName = nameof(RabbitMqInitializationCompleted), Level = LogLevel.Information,
		Message = "RabbitMQ initialization completed: delivery {DeliveryQueue}, retry {RetryQueue}, dead-letter {DeadLetterQueue}")]
	public static partial void RabbitMqInitializationCompleted(this ILogger logger, string deliveryQueue, string retryQueue, string deadLetterQueue);

	[LoggerMessage(EventId = 2002, EventName = nameof(RabbitMqInitializationFailed), Level = LogLevel.Error,
		Message = "RabbitMQ initialization failed: delivery {DeliveryQueue}, retry {RetryQueue}, dead-letter {DeadLetterQueue}")]
	public static partial void RabbitMqInitializationFailed(this ILogger logger, Exception exception, string deliveryQueue, string retryQueue, string deadLetterQueue);
}
