namespace WebhookGateway.API.Application.Webhooks;

public sealed record WebhookEventMetadata(string? DeliveryId, string? EventType);