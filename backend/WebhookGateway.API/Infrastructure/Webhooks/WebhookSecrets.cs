namespace WebhookGateway.API.Infrastructure.Webhooks;

public sealed class WebhookSecrets
{
	public static readonly string SectionName = "WebhookSecrets";
	public Dictionary<string, string> Sources { get; init; } = [];
}