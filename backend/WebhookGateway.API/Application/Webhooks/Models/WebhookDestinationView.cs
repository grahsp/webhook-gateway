namespace WebhookGateway.API.Application.Webhooks.Models;

public sealed record WebhookDestinationView(Guid Id, Guid WebhookRouteId, string Url);