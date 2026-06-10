using WebhookGateway.API.Application.Providers;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookIngestor(WebhookProviderResolver resolver, AppDbContext db, TimeProvider time) : IWebhookIngestor
{
	public async Task Ingest(string providerName, IncomingWebhookRequest request)
	{
		var provider = resolver.Resolve(providerName);
		var metadata = provider.ExtractMetadata(request);
		
		var webhookEvent = WebhookEvent.New(
			Guid.NewGuid(),
			metadata,
			request.Payload,
			time.GetUtcNow());
		
		db.WebhookEvents.Add(webhookEvent);
		await db.SaveChangesAsync();
	}
}