using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Providers;

public class GithubWebhookProvider : IWebhookProvider
{
	public string Name => "github";
	
	public WebhookEventMetadata ExtractMetadata(HttpRequest request)
	{
		request.Headers.TryGetValue("X-GitHub-Delivery", out var delivery);
		request.Headers.TryGetValue("X-GitHub-Event", out var type);
		
		return new WebhookEventMetadata(Name, delivery, type);
	}
}