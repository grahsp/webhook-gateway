using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Application.Sources;

public sealed class TestWebhookSourceHandler : IWebhookSourceHandler
{
	public WebhookSource Source => WebhookSource.Test;

	public WebhookEventMetadata ExtractMetadata(IncomingWebhookRequest request)
	{
		request.Headers.TryGetValue("X-Test-Delivery", out var delivery);
		request.Headers.TryGetValue("X-Test-Event", out var type);

		return new WebhookEventMetadata(delivery, type);
	}

	public void ValidateSignature(IncomingWebhookRequest request) {}
}