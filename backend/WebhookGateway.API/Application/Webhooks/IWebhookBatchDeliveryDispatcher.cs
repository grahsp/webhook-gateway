namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookBatchDeliveryDispatcher
{
	Task DispatchAsync(IEnumerable<Guid> webhookDeliveryId, CancellationToken ct = default);
}