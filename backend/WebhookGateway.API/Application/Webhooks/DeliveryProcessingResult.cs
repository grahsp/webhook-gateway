namespace WebhookGateway.API.Application.Webhooks;

public sealed record DeliveryProcessingResult(Guid DeliveryId, DeliveryAction Action);