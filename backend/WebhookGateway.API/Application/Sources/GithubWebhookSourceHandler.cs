using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Application.Sources;

public class GithubWebhookSourceHandler : IWebhookSourceHandler
{
	public WebhookSource Source => WebhookSource.GitHub;
	
	public WebhookEventMetadata ExtractMetadata(IncomingWebhookRequest request)
	{
		request.Headers.TryGetValue("X-GitHub-Delivery", out var delivery);
		request.Headers.TryGetValue("X-GitHub-Event", out var type);
		
		return new WebhookEventMetadata(delivery, type);
	}
}