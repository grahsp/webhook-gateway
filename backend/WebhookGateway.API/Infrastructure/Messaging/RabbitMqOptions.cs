namespace WebhookGateway.API.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
	public const string SectionName = "RabbitMq";

	public required string Host { get; init; }
	public required string Username { get; init; }
	public required string Password { get; init; }

	public string QueueName { get; init; } = "webhook-deliveries";
}