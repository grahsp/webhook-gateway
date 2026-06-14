namespace WebhookGateway.API.Application.Exceptions;

public class UnauthorizedException(string message) : ApplicationException(message);