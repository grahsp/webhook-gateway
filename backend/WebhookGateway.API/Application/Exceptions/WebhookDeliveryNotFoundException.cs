namespace WebhookGateway.API.Application.Exceptions;

public sealed class WebhookDeliveryNotFoundException(Guid id) : NotFoundException($"Webhook delivery with id '{id}' not found.");