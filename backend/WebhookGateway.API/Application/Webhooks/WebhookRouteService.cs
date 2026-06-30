using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Application.Exceptions;
using WebhookGateway.API.Application.Webhooks.Models;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Logging;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class WebhookRouteService(
	AppDbContext db,
	ILogger<WebhookRouteService> logger)
	: IWebhookRouteService
{
	public async Task<WebhookRouteView> Create(CreateWebhookRouteRequest request)
	{
		var route = WebhookRoute.New(request.Name, request.Source);
		
		db.WebhookRoutes.Add(route);
		await db.SaveChangesAsync();
		
		logger.WebhookRouteCreated(route.Id, route.Source);
		
		return new WebhookRouteView(route.Id, route.Name, route.Source);
	}

	public async Task<WebhookRouteView> Get(Guid id)
		=> await db.WebhookRoutes
				.Where(r => r.Id == id)
				.Select(r => new WebhookRouteView(r.Id, r.Name, r.Source))
				.FirstOrDefaultAsync()
			?? throw new WebhookRouteNotFoundException(id);

	public async Task<IReadOnlyList<WebhookRouteView>> List()
		=> await db.WebhookRoutes
			.Select(r => new WebhookRouteView(r.Id, r.Name, r.Source))
			.ToListAsync();

	public async Task Delete(Guid id)
	{
		var route = await db.WebhookRoutes.SingleOrDefaultAsync(x => x.Id == id)
			?? throw new WebhookRouteNotFoundException(id);

		db.WebhookRoutes.Remove(route);
		await db.SaveChangesAsync();
		
		logger.WebhookRouteDeleted(route.Id);
	}
}
