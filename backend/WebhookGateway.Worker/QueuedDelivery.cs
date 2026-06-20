namespace WebhookGateway.Worker;

public sealed record QueuedDelivery(Guid DeliveryId, ulong DeliveryTag);