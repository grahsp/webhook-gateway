using WebhookGateway.API.Domain.Exceptions;

namespace WebhookGateway.API.Domain;

public sealed class WebhookDestination
{
	public Guid Id { get; private set; }
	public Guid WebhookRouteId { get; private set; }
	public string Url { get; private set; } = null!;
	
	private WebhookDestination() {}

	private WebhookDestination(Guid webhookRouteId, string url)
	{
		Id = Guid.NewGuid();
		WebhookRouteId = webhookRouteId;
		Url = url;
	}

	internal static WebhookDestination New(Guid webhookRouteId, string url)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(url);
		
		if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
			throw new InvalidDestinationUrlException(url);
		
		if (uri.Scheme != "http" && uri.Scheme != "https")
			throw new InvalidDestinationUrlException(url);
		
		return new WebhookDestination(webhookRouteId, uri.AbsoluteUri);
	}
}