using System.Text;
using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Application.Exceptions;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookDeliveryDispatcher(
	AppDbContext db,
	HttpClient http,
	TimeProvider time)
	: IWebhookDeliveryDispatcher
{
	public async Task Dispatch(Guid webhookEventId)
	{
		var @event = await db.WebhookEvents
				.Where(x => x.Id == webhookEventId)
				.Include(x => x.Deliveries)
				.ThenInclude(x => x.WebhookDestination)
				.FirstOrDefaultAsync()
			?? throw new WebhookEventNotFoundException(webhookEventId);
		
		foreach (var delivery in @event.Deliveries)
		{
			delivery.MarkStarted(time.GetUtcNow());

			try
			{
				var content = new StringContent(@event.Payload, Encoding.UTF8, "application/json");
				var response = await http.PostAsync(delivery.WebhookDestination.Url, content);
				
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
		
		await db.SaveChangesAsync();
	}
}