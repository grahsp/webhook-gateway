using Microsoft.Extensions.Options;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Infrastructure.Messaging;
using WebhookGateway.API.Infrastructure.Webhooks;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookDeliveryRetryPolicy(
	IOptions<RabbitMqOptions> options)
	: IWebhookDeliveryRetryPolicy
{
	private readonly RabbitMqOptions _options = options.Value;

	public DeliveryAction Decide(WebhookDelivery delivery, DeliveryDispatchResult result)
	{
		if (result.Type == DeliveryResultType.Success)
			return DeliveryAction.Ack;

		if (result.Type == DeliveryResultType.PermanentFailure)
			return DeliveryAction.DeadLetter;

		if (delivery.Attempts.Count >= _options.MaxRetryAttempts)
			return DeliveryAction.DeadLetter;

		return DeliveryAction.Retry;
	}
}