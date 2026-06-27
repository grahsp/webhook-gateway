using WebhookGateway.API.Domain;
using WebhookGateway.API.Infrastructure.Webhooks;

namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookDeliveryRetryPolicy
{
	DeliveryAction Decide(
		WebhookDelivery delivery,
		DeliveryDispatchResult result);
}