namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookIngestor
{
	Task Ingest(Guid webhookRouteId, IncomingWebhookRequest request);
}