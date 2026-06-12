using WebhookGateway.API.Application.Webhooks.Models;

namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookRouteService
{
	Task<WebhookRouteView> Create(CreateWebhookRouteRequest request);
	Task<WebhookRouteView> Get(Guid id);
	Task<IReadOnlyList<WebhookRouteView>> List();
	Task Delete(Guid id);
}