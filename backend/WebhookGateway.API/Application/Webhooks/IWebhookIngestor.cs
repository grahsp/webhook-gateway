namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookIngestor
{
	Task Ingest(string providerName, IncomingWebhookRequest request);
}