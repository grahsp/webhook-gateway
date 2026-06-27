namespace WebhookGateway.API.Application.Webhooks;

public interface IWebhookBatchDeliveryDispatcher
{
	Task<IReadOnlyList<DeliveryProcessingResult>> DispatchAsync(IEnumerable<Guid> webhookDeliveryId, CancellationToken ct = default);
}