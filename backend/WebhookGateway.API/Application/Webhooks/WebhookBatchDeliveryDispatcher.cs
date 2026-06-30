using System.Text;
using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Infrastructure.Webhooks;
using WebhookGateway.API.Logging;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookBatchDeliveryDispatcher(
	AppDbContext db,
	IWebhookDeliveryFailureClassifier classifier,
	IWebhookDeliveryRetryPolicy retryPolicy,
	IHttpClientFactory http,
	ILogger<WebhookBatchDeliveryDispatcher> logger,
	TimeProvider time)
	: IWebhookBatchDeliveryDispatcher
{
	public async Task<IReadOnlyList<DeliveryProcessingResult>> DispatchAsync(IEnumerable<Guid> webhookDeliveryId, CancellationToken ct = default)
	{
		var deliveries = await db.WebhookDeliveries
			.Where(x => webhookDeliveryId.Contains(x.Id))
			.Include(x => x.WebhookEvent)
			.Include(x => x.WebhookDestination)
			.Include(x => x.Attempts)
			.ToListAsync(ct);
		
		var client = http.CreateClient();
		var results = new List<DeliveryProcessingResult>();

		var started = new List<WebhookDelivery>();
		foreach (var delivery in deliveries)
		{
			if (!delivery.TryStartAttempt(time.GetUtcNow(), out var attempt))
				continue;

			started.Add(delivery);
			db.Add(attempt);
		}
		
		await db.SaveChangesAsync(ct);
		

		foreach (var delivery in started)
		{
			DeliveryDispatchResult classification;
			
			try
			{
				var content = new StringContent(delivery.WebhookEvent.Payload, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(delivery.WebhookDestination.Url, content, ct);
				
				classification = classifier.Classify(delivery.Id, response);
			}
			catch(Exception ex)
			{
				logger.DeliveryDispatchException(
					ex,
					delivery.Id,
					delivery.WebhookDestinationId,
					delivery.Attempts.Count);
				classification = classifier.Classify(delivery.Id, ex);
			}
			
			var processed = ApplyResult(delivery, classification);
			LogDeliveryOutcome(delivery, classification, processed.Action);
			results.Add(processed);
		}
		
		await db.SaveChangesAsync(ct);
		return results;
	}
	
	private DeliveryProcessingResult ApplyResult(WebhookDelivery delivery, DeliveryDispatchResult result)
	{
		var action = retryPolicy.Decide(delivery, result);

		switch (action)
		{
			case DeliveryAction.Ack:
				delivery.MarkSucceeded(result.StatusCode!.Value, time.GetUtcNow());
				break;

			case DeliveryAction.Retry:
				delivery.MarkAttemptFailed(result.StatusCode, result.ErrorMessage, time.GetUtcNow());
				break;

			case DeliveryAction.DeadLetter:
				delivery.MarkAttemptFailed(result.StatusCode, result.ErrorMessage, time.GetUtcNow());
				delivery.MarkFailed();
				break;
		}

		return new DeliveryProcessingResult(delivery.Id, action);
	}
	
	private void LogDeliveryOutcome(WebhookDelivery delivery, DeliveryDispatchResult result, DeliveryAction action)
	{
		switch (action)
		{
			case DeliveryAction.Ack:
				logger.DeliveryCompleted(
					delivery.Id,
					delivery.WebhookDestinationId,
					result.StatusCode,
					delivery.Attempts.Count,
					action);
				break;

			case DeliveryAction.Retry:
				if (result.StatusCode is >= 500 and <= 599)
				{
					logger.EndpointReturned5xxRetrying(
						delivery.Id,
						delivery.WebhookDestinationId,
						result.StatusCode.Value,
						delivery.Attempts.Count);
				}
				else
				{
					logger.DeliveryRetryScheduled(
						delivery.Id,
						delivery.WebhookDestinationId,
						result.StatusCode,
						delivery.Attempts.Count);
				}
				break;

			case DeliveryAction.DeadLetter:
				logger.DeliveryDeadLettered(
					delivery.Id,
					delivery.WebhookDestinationId,
					result.StatusCode,
					delivery.Attempts.Count,
					result.Type);
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(action));
		}
	}
}
