namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookDeliveryDispatcher
{
	Task DispatchAsync(Guid webhookDeliveryId);
}