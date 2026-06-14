namespace WebhookGateway.API.Application.Exceptions;

public class NotFoundException(string message) : ApplicationException(message);