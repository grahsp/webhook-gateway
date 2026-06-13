using WebhookGateway.API.Domain.Exceptions;

namespace WebhookGateway.API.Domain;

public sealed class WebhookRoute
{
	public Guid Id { get; private set; }
	public string Name { get; private set; } = null!;
	public WebhookSource Source { get; private set; }

	private List<WebhookDestination> _destinations = [];
	public IReadOnlyCollection<WebhookDestination> Destinations => _destinations;

	private WebhookRoute() {}

	private WebhookRoute(string name, WebhookSource source)
	{
		Id = Guid.NewGuid();
		Name = name;
		Source = source;
	}

	public static WebhookRoute New(string name, WebhookSource source)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return new WebhookRoute(name, source);
	}

	public WebhookDestination AddDestination(string url)
	{
		var destination = WebhookDestination.New(Id, url);
		
		if (_destinations.Any(x => x.Url == destination.Url))
			throw new DuplicateDestinationException(destination.Url);
		
		_destinations.Add(destination);
		return destination;
	}
	
	public void RemoveDestination(Guid destinationId)
	{
		var destination = _destinations
				.SingleOrDefault(x => x.Id == destinationId)
			?? throw new DestinationDoesNotBelongToRouteException();

		_destinations.Remove(destination);
	}
}