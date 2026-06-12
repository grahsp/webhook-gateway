namespace WebhookGateway.API.Domain.Exceptions;

public sealed class InvalidDestinationUrlException(string url) : DomainException($"'{url}' is not a valid destination URL.");