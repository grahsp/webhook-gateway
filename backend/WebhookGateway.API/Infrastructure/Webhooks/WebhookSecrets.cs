using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Infrastructure.Webhooks;

public sealed class WebhookSecrets
{
	public static readonly string SectionName = "WebhookSecrets";

	public Dictionary<string, string> Sources { get; init; } = [];
	
	public bool IsValid()
	{
		var sources = Enum.GetNames<WebhookSource>()
			.Where(x => x != nameof(WebhookSource.Unknown));

		foreach (var source in sources)
		{
			if (!Sources.TryGetValue(source, out var secret))
				return false;

			if (string.IsNullOrWhiteSpace(secret))
				return false;
		}

		return true;
	}
}