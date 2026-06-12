using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Application.Providers;

public sealed class WebhookSourceResolver
{
	private readonly IReadOnlyDictionary<WebhookSource, IWebhookSourceHandler> _sources;

	public WebhookSourceResolver(IEnumerable<IWebhookSourceHandler> sources)
	{
		_sources = sources.ToDictionary(p => p.Source);
	}

	public IWebhookSourceHandler Resolve(WebhookSource source)
		=> _sources.GetValueOrDefault(source) ?? throw new InvalidOperationException($"Source '{source}' is not supported.");
}