using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Application.Webhooks.Models;

public sealed record CreateWebhookRouteRequest(string Name, WebhookSource Source);