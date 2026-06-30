using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebhookGateway.API.Application.Exceptions;
using WebhookGateway.API.Application.Sources;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Infrastructure.Extensions;
using WebhookGateway.API.Infrastructure.Messaging;
using WebhookGateway.API.Infrastructure.Metrics;
using WebhookGateway.API.Logging;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookIngestor(
	WebhookSourceResolver resolver,
	IMessagePublisher queue,
	IOptions<RabbitMqOptions> options,
	AppDbContext db,
	IngestionMetrics metrics,
	ILogger<WebhookIngestor> logger,
	TimeProvider time)
	: IWebhookIngestor
{
	private readonly RabbitMqOptions _options = options.Value;
	
	public async Task Ingest(Guid webhookRouteId, IncomingWebhookRequest request)
	{
		var started = Stopwatch.GetTimestamp();
		
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
		
		metrics.WebhookReceived(route.Source.ToString(), metadata.EventType ?? "unknown");

		foreach (var destination in route.Destinations)
			webhookEvent.AddDelivery(destination.Id, time.GetUtcNow());

		try
		{
			await db.SaveChangesAsync();
			logger.WebhookReceived(
				route.Id,
				webhookEvent.Id,
				metadata.DeliveryId ?? "unknown",
				route.Source,
				metadata.EventType ?? "unknown",
				webhookEvent.Deliveries.Count);
		}
		catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
		{
			logger.DuplicateWebhookIgnored(
				route.Id,
				metadata.DeliveryId ?? "unknown",
				route.Source,
				metadata.EventType ?? "unknown");
			return;
		}

		foreach (var delivery in webhookEvent.Deliveries)
		{
			try
			{
				await queue.PublishAsync(_options.DeliveryQueue, delivery.Id);
				logger.DeliveryQueued(webhookEvent.Id, delivery.Id, _options.DeliveryQueue);
			}
			catch(Exception ex)
			{
				logger.WebhookEnqueueFailed(ex, webhookEvent.Id, delivery.Id, _options.DeliveryQueue);
				throw;
			}
		}
		
		metrics.RecordIngestionDuration(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
	}
}
