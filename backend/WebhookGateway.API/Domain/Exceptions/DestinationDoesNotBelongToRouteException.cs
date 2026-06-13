namespace WebhookGateway.API.Domain.Exceptions;

public sealed class DestinationDoesNotBelongToRouteException()
	: DomainException("Destination does not belong to this route.");