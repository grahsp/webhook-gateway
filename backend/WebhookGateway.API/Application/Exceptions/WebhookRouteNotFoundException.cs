namespace WebhookGateway.API.Application.Exceptions;

public sealed class WebhookRouteNotFoundException(Guid id): NotFoundException($"Webhook route with id '{id}' was not found.");