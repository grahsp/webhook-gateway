using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Application.Providers;

public interface IWebhookProvider
{
	string Name { get; }
	WebhookEventMetadata ExtractMetadata(IncomingWebhookRequest request);
}