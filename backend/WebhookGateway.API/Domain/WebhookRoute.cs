namespace WebhookGateway.API.Domain;

public sealed class WebhookRoute
{
	public Guid Id { get; private set; }
	public string Name { get; private set; } = null!;
	public WebhookSource Source { get; private set; }

	private WebhookRoute() {}

	public WebhookRoute(string name, WebhookSource source)
	{
		Id = Guid.NewGuid();
		Name = name;
		Source = source;
	}
}