namespace WebhookGateway.API.Application.Exceptions;

public sealed class InvalidWebhookSignatureException() : UnauthorizedException("The webhook signature is invalid");