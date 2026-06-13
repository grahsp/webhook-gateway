using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Application.Exceptions;
using WebhookGateway.API.Application.Webhooks.Models;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public class WebhookDestinationService(
	AppDbContext db,
	ILogger<WebhookDestinationService> logger)
	: IWebhookDestinationService
{
	public async Task<WebhookDestinationView> Create(Guid webhookRouteId, CreateWebhookDestinationRequest request)
	{
		var route = await db.WebhookRoutes
				.Where(r => r.Id == webhookRouteId)
				.Include(r => r.Destinations)
				.FirstOrDefaultAsync()
			?? throw new WebhookRouteNotFoundException(webhookRouteId);
		
		var destination = route.AddDestination(request.Url);
		
		db.WebhookDestinations.Add(destination);
		await db.SaveChangesAsync();
		
		logger.LogInformation("Created WebhookDestination with id '{Id}' for WebhookRoute '{RouteId}'", destination.Id, webhookRouteId);
		
		return new WebhookDestinationView(destination.Id, destination.WebhookRouteId, destination.Url);
	}

	public async Task<WebhookDestinationView> Get(Guid webhookRouteId, Guid id)
		=> await db.WebhookDestinations
			.Where(d => d.Id == id && d.WebhookRouteId == webhookRouteId)
			.Select(d => new WebhookDestinationView(d.Id, d.WebhookRouteId, d.Url))
			.FirstOrDefaultAsync() ?? throw new WebhookDestinationNotFoundException(id);

	public async Task<IReadOnlyList<WebhookDestinationView>> List(Guid webhookRouteId)
		=> await db.WebhookDestinations
			.Where(d => d.WebhookRouteId == webhookRouteId)
			.Select(d => new WebhookDestinationView(d.Id, d.WebhookRouteId, d.Url))
			.ToListAsync();

	public async Task Delete(Guid webhookRouteId, Guid id)
	{
		var route = await db.WebhookRoutes
				.Where(r => r.Id == webhookRouteId)
				.Include(r => r.Destinations)
				.FirstOrDefaultAsync()
			?? throw new WebhookRouteNotFoundException(webhookRouteId);

		route.RemoveDestination(id);
		await db.SaveChangesAsync();
	}
}