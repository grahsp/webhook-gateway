using System.Security.Cryptography;
using System.Text;
using WebhookGateway.API.Application.Exceptions;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Infrastructure.Webhooks;

namespace WebhookGateway.API.Application.Sources;

public sealed class GithubWebhookSourceHandler(
	IWebhookSourceSecretProvider secrets,
	ILogger<GithubWebhookSourceHandler> logger)
	: IWebhookSourceHandler
{
	public WebhookSource Source => WebhookSource.GitHub;

	public WebhookEventMetadata ExtractMetadata(IncomingWebhookRequest request)
	{
		request.Headers.TryGetValue("X-GitHub-Delivery", out var delivery);
		request.Headers.TryGetValue("X-GitHub-Event", out var type);

		return new WebhookEventMetadata(delivery, type);
	}

	public void ValidateSignature(IncomingWebhookRequest request)
	{
		var signature = request.Headers.GetValueOrDefault("X-Hub-Signature-256")
			?? throw new InvalidWebhookSignatureException();
		
		if (!signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
			throw new InvalidWebhookSignatureException();

		var secret = secrets.GetSecret(Source);
		using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));

		try
		{
			var expected = hmac.ComputeHash(request.PayloadBytes);
			var provided = Convert.FromHexString(signature["sha256=".Length..]);

			if (!CryptographicOperations.FixedTimeEquals(expected, provided))
			{
				logger.LogWarning("Invalid signature for source '{Source}'", Source);
				throw new InvalidWebhookSignatureException();
			}
		}
		catch (FormatException)
		{
			throw new InvalidWebhookSignatureException();
		}
	}
}