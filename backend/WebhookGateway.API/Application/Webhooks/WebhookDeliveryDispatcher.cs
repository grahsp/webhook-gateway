using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebhookGateway.API.Application.Exceptions;
using WebhookGateway.API.Infrastructure.Messaging;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookDeliveryDispatcher(
	AppDbContext db,
	IOptions<RabbitMqOptions> options,
	IHttpClientFactory http,
	TimeProvider time)
	: IWebhookDeliveryDispatcher
{
	private readonly RabbitMqOptions _options = options.Value;
	
	public async Task DispatchAsync(Guid webhookDeliveryId)
	{
		var delivery = await db.WebhookDeliveries
				.Where(x => x.Id == webhookDeliveryId)
				.Include(x => x.WebhookEvent)
				.Include(x => x.WebhookDestination)
				.FirstOrDefaultAsync()
			?? throw new WebhookDeliveryNotFoundException(webhookDeliveryId);
		
		delivery.StartAttempt(_options.MaxRetryAttempts, time.GetUtcNow());

		try
		{
			var client = http.CreateClient();
			
			var content = new StringContent(delivery.WebhookEvent.Payload, Encoding.UTF8, "application/json");
			var response = await client.PostAsync(delivery.WebhookDestination.Url, content);
				
			if (response.IsSuccessStatusCode)
				delivery.MarkSucceeded((int)response.StatusCode, time.GetUtcNow());
			else
				delivery.MarkFailed((int)response.StatusCode, response.ReasonPhrase, time.GetUtcNow());
		}
		catch(Exception ex)
		{
			delivery.MarkFailed(null, ex.Message, time.GetUtcNow());
		}
		
		await db.SaveChangesAsync();
	}
}