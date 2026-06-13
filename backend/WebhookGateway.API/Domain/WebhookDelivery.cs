namespace WebhookGateway.API.Domain;

public sealed class WebhookDelivery
{
	public Guid Id { get; private set; }
	
	public Guid WebhookEventId { get; private set; }
	public WebhookEvent WebhookEvent { get; private set; } = null!;
	
	public Guid WebhookDestinationId { get; private set; }
	public WebhookDestination WebhookDestination { get; private set; } = null!;
	
	public DeliveryStatus Status { get; private set; } = DeliveryStatus.Pending;
	
	public DateTimeOffset CreatedAt { get; private set; }
	public DateTimeOffset? StartedAt { get; private set; }
	public DateTimeOffset? DeliveredAt { get; private set; }
	public DateTimeOffset? FailedAt { get; private set; }
	
	private WebhookDelivery() {}

	internal WebhookDelivery(Guid eventId, Guid destinationId, DateTimeOffset now)
	{
		WebhookEventId = eventId;
		WebhookDestinationId = destinationId;
		CreatedAt = now;
	}

	public void MarkStarted(DateTimeOffset now)
	{
		if (Status != DeliveryStatus.Pending)
			throw new InvalidOperationException("Delivery already started");
		
		Status = DeliveryStatus.InProgress;
		StartedAt = now;
	}

	public void MarkSucceeded(DateTimeOffset now)
	{
		if (Status != DeliveryStatus.InProgress)
			throw new InvalidOperationException("Delivery not in progress");
		
		Status = DeliveryStatus.Succeeded;
		DeliveredAt = now;
	}
	
	public void MarkFailed(DateTimeOffset now)
	{
		if (Status != DeliveryStatus.InProgress)
			throw new InvalidOperationException("Delivery not in progress");
		
		Status = DeliveryStatus.Failed;
		FailedAt = now;
	}
}