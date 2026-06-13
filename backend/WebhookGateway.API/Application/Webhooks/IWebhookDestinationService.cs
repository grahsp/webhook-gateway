using WebhookGateway.API.Application.Webhooks.Models;

namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookDestinationService
{
	Task<WebhookDestinationView> Create(Guid webhookRouteId, CreateWebhookDestinationRequest request);
	Task<WebhookDestinationView> Get(Guid webhookRouteId, Guid id);
	Task<IReadOnlyList<WebhookDestinationView>> List(Guid webhookRouteId);
	Task Delete(Guid webhookRouteId, Guid id);
}