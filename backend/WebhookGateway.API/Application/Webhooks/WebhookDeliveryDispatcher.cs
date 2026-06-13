using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookDeliveryDispatcher(
	AppDbContext db,
	TimeProvider time)
	: IWebhookDeliveryDispatcher
{
	public async Task Dispatch(Guid webhookEventId)
	{
		var deliveries = await db.WebhookDeliveries
			.Where(d => d.WebhookEventId == webhookEventId)
			.Where(d => d.Status == DeliveryStatus.Pending)
			.ToListAsync();

		foreach (var delivery in deliveries)
			delivery.MarkSucceeded(time.GetUtcNow());
		
		await db.SaveChangesAsync();
	}
}