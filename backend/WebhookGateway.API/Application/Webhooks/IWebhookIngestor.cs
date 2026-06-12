using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookIngestor
{
	Task Ingest(WebhookSource source, IncomingWebhookRequest request);
}