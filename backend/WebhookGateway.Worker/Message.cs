namespace WebhookGateway.Worker;

public sealed record Message<T>(T Body, ulong DeliveryTag);