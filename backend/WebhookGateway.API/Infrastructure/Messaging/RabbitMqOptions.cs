namespace WebhookGateway.API.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
	public const string SectionName = "RabbitMq";

	public required string Host { get; init; }
	public required string Username { get; init; }
	public required string Password { get; init; }

	public string DeliveryQueue { get; init; } = "webhook-deliveries";
	public string RetryQueue { get; init; } = "webhook-deliveries-retry";
	public string DeadLetterQueue { get; init; } = "webhook-deliveries-dlq";
	
	public int RetryAttempts { get; init; } = 5;
	public TimeSpan RetryDelaySeconds { get; init; } = TimeSpan.FromSeconds(30);
}