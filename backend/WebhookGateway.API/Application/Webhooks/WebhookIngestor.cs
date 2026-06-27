using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebhookGateway.API.Application.Exceptions;
using WebhookGateway.API.Application.Sources;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Infrastructure.Extensions;
using WebhookGateway.API.Infrastructure.Messaging;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookIngestor(
	WebhookSourceResolver resolver,
	IMessagePublisher queue,
	IOptions<RabbitMqOptions> options,
	AppDbContext db,
	ILogger<WebhookIngestor> logger,
	TimeProvider time)
	: IWebhookIngestor
{
	private readonly RabbitMqOptions _options = options.Value;
	
	public async Task Ingest(Guid webhookRouteId, IncomingWebhookRequest request)
	{
		var route = await db.WebhookRoutes
				.Where(r => r.Id == webhookRouteId)
				.Include(r => r.Destinations)
				.FirstOrDefaultAsync()
			?? throw new WebhookRouteNotFoundException(webhookRouteId);
		
		var handler = resolver.Resolve(route.Source);
		
		handler.ValidateSignature(request);
		var metadata = handler.ExtractMetadata(request);
		
		var webhookEvent = WebhookEvent.New(
			route.Id,
			metadata.DeliveryId,
			metadata.EventType,
			request.Payload,
			time.GetUtcNow());

		db.WebhookEvents.Add(webhookEvent);

		foreach (var destination in route.Destinations)
			webhookEvent.AddDelivery(destination.Id, time.GetUtcNow());

		try
		{
			await db.SaveChangesAsync();
			logger.LogInformation("Webhook received: {DeliveryId}", metadata.DeliveryId);
		}
		catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
		{
			logger.LogInformation("Duplicate webhook received: {DeliveryId}", metadata.DeliveryId);
			return;
		}

		try
		{
			foreach (var delivery in webhookEvent.Deliveries)
				await queue.PublishAsync(_options.DeliveryQueue, delivery.Id);
		}
		catch(Exception ex)
		{
			logger.LogError(ex, "Failed to enqueue deliveries for event {EventId}", webhookEvent.Id);
			throw;
		}
	}
}