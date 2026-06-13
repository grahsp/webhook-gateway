namespace WebhookGateway.API.Domain;

public sealed class WebhookDelivery
{
	public Guid Id { get; private set; }
	
	public Guid WebhookEventId { get; private set; }
	public Guid WebhookDestinationId { get; private set; }
	
	public DeliveryStatus Status { get; private set; } = DeliveryStatus.Pending;
	
	public DateTimeOffset CreatedAt { get; private set; }
	public DateTimeOffset? DeliveredAt { get; private set; }
	
	private WebhookDelivery() {}

	internal WebhookDelivery(Guid eventId, Guid destinationId, DateTimeOffset now)
	{
		WebhookEventId = eventId;
		WebhookDestinationId = destinationId;
		CreatedAt = now;
	}

	public void MarkSucceeded(DateTimeOffset now)
	{
		Status = DeliveryStatus.Succeeded;
		DeliveredAt = now;
	}
}