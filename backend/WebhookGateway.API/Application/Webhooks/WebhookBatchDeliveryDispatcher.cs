using System.Text;
using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Infrastructure.Webhooks;
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
				logger.LogWarning(ex, "Failed to dispatch delivery {Exception}", ex.Message);
				classification = classifier.Classify(delivery.Id, ex);
			}
			
			var processed = ApplyResult(delivery, classification);
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
}