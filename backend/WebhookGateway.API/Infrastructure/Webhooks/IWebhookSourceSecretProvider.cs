using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Infrastructure.Webhooks;

public interface IWebhookSourceSecretProvider
{
	string GetSecret(WebhookSource source);
}