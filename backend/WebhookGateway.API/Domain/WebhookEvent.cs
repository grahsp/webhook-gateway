namespace WebhookGateway.API.Domain;

public sealed class WebhookEvent{
	public Guid Id { get; private set; }
	public WebhookEventMetadata Metadata { get; private set; } = null!;
	public string Payload { get; private set; } = null!;
	public DateTimeOffset ReceivedAt { get; private set; }
	
	private WebhookEvent() { }

	private WebhookEvent(WebhookEventMetadata metadata, string payload, DateTimeOffset receivedAt)
	{
		Id = Guid.NewGuid();
		Metadata = metadata;
		Payload = payload;
		ReceivedAt = receivedAt;
	}
	
	public static WebhookEvent New(WebhookEventMetadata metadata, string payload, DateTimeOffset receivedAt)
		=> new WebhookEvent(metadata, payload, receivedAt);
}