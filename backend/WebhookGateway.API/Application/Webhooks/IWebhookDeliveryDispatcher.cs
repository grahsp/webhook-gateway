namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookDeliveryDispatcher
{
	Task Dispatch(Guid webhookEventId);
}