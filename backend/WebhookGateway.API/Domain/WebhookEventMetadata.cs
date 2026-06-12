namespace WebhookGateway.API.Domain;

public sealed record WebhookEventMetadata(WebhookSource Source, string? DeliveryId, string? EventType);