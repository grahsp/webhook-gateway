using System.Text;
using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookBatchDeliveryDispatcher(
	AppDbContext db,
	IHttpClientFactory http,
	TimeProvider time )
	: IWebhookBatchDeliveryDispatcher
{
	public async Task DispatchAsync(IEnumerable<Guid> webhookDeliveryId, CancellationToken ct = default)
	{
		var deliveries = await db.WebhookDeliveries
			.Where(x => webhookDeliveryId.Contains(x.Id))
			.Include(x => x.WebhookEvent)
			.Include(x => x.WebhookDestination)
			.ToListAsync(ct);
		
		var client = http.CreateClient();

		foreach (var delivery in deliveries) {
			try
			{
				delivery.MarkStarted(time.GetUtcNow());
				
				var content = new StringContent(delivery.WebhookEvent.Payload, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(delivery.WebhookDestination.Url, content, ct);
				
				if (response.IsSuccessStatusCode)
					delivery.MarkSucceeded((int)response.StatusCode, time.GetUtcNow());
				else
					delivery.MarkFailed((int)response.StatusCode, response.ReasonPhrase, time.GetUtcNow());
			}
			catch(Exception ex)
			{
				delivery.MarkFailed(null, ex.Message, time.GetUtcNow());
			}
		}
		
		await db.SaveChangesAsync(ct);
	}
}