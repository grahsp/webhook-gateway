namespace WebhookGateway.API.Providers;

public sealed class WebhookProviderResolver
{
	private readonly IReadOnlyDictionary<string, IWebhookProvider> _providers;

	public WebhookProviderResolver(IEnumerable<IWebhookProvider> providers)
	{
		_providers = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
	}

	public IWebhookProvider Resolve(string provider)
		=> _providers.GetValueOrDefault(provider) ?? throw new InvalidOperationException($"No provider found for '{provider}'");
}