namespace WebhookGateway.API.Application.Exceptions;

public sealed class WebhookEventNotFoundException(Guid id) : NotFoundException($"Webhook event with id '{id}' not found.");