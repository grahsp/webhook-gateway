namespace WebhookGateway.API.Domain;

public sealed record WebhookEventMetadata(string Provider, string? DeliveryId, string? EventType);