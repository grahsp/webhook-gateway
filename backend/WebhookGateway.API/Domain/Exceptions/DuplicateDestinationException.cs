namespace WebhookGateway.API.Domain.Exceptions;

public sealed class DuplicateDestinationException(string url) : DomainException($"Destination '{url}' already exists.");