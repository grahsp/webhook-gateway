using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Providers;

public interface IWebhookProvider
{
	string Name { get; }
	WebhookEventMetadata ExtractMetadata(HttpRequest request);
}