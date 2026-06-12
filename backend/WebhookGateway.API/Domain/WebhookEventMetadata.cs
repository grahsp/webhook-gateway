namespace WebhookGateway.API.Domain;

public sealed record WebhookEventMetadata(string? DeliveryId, string? EventType);