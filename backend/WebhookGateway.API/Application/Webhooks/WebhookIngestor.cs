using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Application.Sources;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Infrastructure.Extensions;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookIngestor(
	WebhookSourceResolver resolver,
	AppDbContext db,
	ILogger<WebhookIngestor> logger,
	TimeProvider time)
	: IWebhookIngestor
{
	public async Task Ingest(WebhookSource source, IncomingWebhookRequest request)
	{
		var handler = resolver.Resolve(source);
		var metadata = handler.ExtractMetadata(request);
		
		var webhookEvent = WebhookEvent.New(
			metadata,
			request.Payload,
			time.GetUtcNow());

		db.WebhookEvents.Add(webhookEvent);

		try
		{
			await db.SaveChangesAsync();
			logger.LogInformation("Webhook received: {DeliveryId}", metadata.DeliveryId);
		}
		catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
		{
			logger.LogInformation("Duplicate webhook received: {DeliveryId}", metadata.DeliveryId);
		}
	}
}