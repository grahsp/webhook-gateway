using Microsoft.Extensions.Options;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Infrastructure.Webhooks;

public sealed class WebhookSourceSecretProvider : IWebhookSourceSecretProvider
{
	private readonly IReadOnlyDictionary<WebhookSource, string> _secrets;

	public WebhookSourceSecretProvider(IOptions<WebhookSecrets> options)
	{
		_secrets = options.Value.Sources.ToDictionary(
			kvp => Enum.Parse<WebhookSource>(kvp.Key, true),
			kvp => kvp.Value);
	}

	public string GetSecret(WebhookSource source)
	{
		if (!_secrets.TryGetValue(source, out var secret))
			throw new InvalidOperationException($"Source '{source}' is not supported.");

		if (string.IsNullOrWhiteSpace(secret))
			throw new InvalidOperationException($"No secret configured for source '{source}'.");

		return secret;
	}
}