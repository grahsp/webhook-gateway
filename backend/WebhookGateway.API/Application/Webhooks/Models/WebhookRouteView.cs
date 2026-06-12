using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Application.Webhooks.Models;

public sealed record WebhookRouteView(Guid Id, string Name, WebhookSource Source);