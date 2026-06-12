using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Application.Sources;

public interface IWebhookSourceHandler
{
	WebhookSource Source { get; }
	WebhookEventMetadata ExtractMetadata(IncomingWebhookRequest request);
}