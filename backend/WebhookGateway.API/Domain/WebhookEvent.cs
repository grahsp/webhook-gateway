namespace WebhookGateway.API.Domain;

public sealed class WebhookEvent{
	public Guid Id { get; private set; }
	public Guid WebhookRouteId { get; private set; }
	public string? DeliveryId { get; private set; }
	public string? EventType { get; private set; }
	public string Payload { get; private set; } = null!;
	public DateTimeOffset ReceivedAt { get; private set; }
	
	private WebhookEvent() { }

	private WebhookEvent(Guid webhookRouteId, string? deliveryId, string? eventType, string payload, DateTimeOffset receivedAt)
	{
		Id = Guid.NewGuid();
		WebhookRouteId = webhookRouteId;
		DeliveryId = deliveryId;
		EventType = eventType;
		Payload = payload;
		ReceivedAt = receivedAt;
	}
	
	public static WebhookEvent New(Guid webhookId, string? deliveryId, string? eventType, string payload, DateTimeOffset receivedAt)
		=> new WebhookEvent(webhookId, deliveryId, eventType, payload, receivedAt);
}