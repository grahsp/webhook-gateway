namespace WebhookGateway.API.Application.Exceptions;

public sealed class WebhookDestinationNotFoundException(Guid id) : NotFoundException($"Webhook destination with id '{id}' not found.");