using WebhookGateway.API.Api.Extensions;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Application.Webhooks.Models;

namespace WebhookGateway.API.Endpoints;

public static class WebhookEndpoints
{
	public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapPost("/webhooks/{routeId}", ReceiveWebhook);

		app.MapPost("/routes", CreateWebhookRoute);
		app.MapGet("/routes", ListWebhookRoutes);
		app.MapGet("/routes/{routeId}", GetWebhookRoute)
			.WithName(nameof(GetWebhookRoute));
		app.MapDelete("/routes/{routeId}", DeleteWebhookRoute);
		
		app.MapPost("/routes/{routeId}/destinations", CreateWebhookDestination);
		app.MapGet("/routes/{routeId}/destinations", ListWebhookDestinations);
		app.MapGet("/routes/{routeId}/destinations/{destinationId}", GetWebhookDestination)
			.WithName(nameof(GetWebhookDestination));
		app.MapDelete("/routes/{routeId}/destinations/{destinationId}", DeleteWebhookDestination);

		return app;
	}

	private static async Task<IResult> ReceiveWebhook(Guid routeId, HttpRequest request, IWebhookIngestor ingestor)
	{
		var data = await request.ExtractWebhookRequest();
		await ingestor.Ingest(routeId, data);

		return Results.Ok();
	}

	private static async Task<IResult> CreateWebhookRoute(CreateWebhookRouteRequest request, IWebhookRouteService service)
	{
		var route = await service.Create(request);
		return Results.CreatedAtRoute(nameof(GetWebhookRoute), new { routeId = route.Id }, route);
	}

	private static async Task<IResult> GetWebhookRoute(Guid routeId, IWebhookRouteService service)
	{
		var route = await service.Get(routeId);
		return Results.Ok(route);
	}

	private static async Task<IResult> ListWebhookRoutes(IWebhookRouteService service)
	{
		var routes = await service.List();
		return Results.Ok(routes);
	}
	
	private static async Task<IResult> DeleteWebhookRoute(Guid routeId, IWebhookRouteService service)
	{
		await service.Delete(routeId);
		return Results.NoContent();
	}
	
	
	private static async Task<IResult> CreateWebhookDestination(Guid routeId, CreateWebhookDestinationRequest request, IWebhookDestinationService service)
	{
		var route = await service.Create(routeId, request);
		return Results.CreatedAtRoute(nameof(GetWebhookDestination), new { routeId = route.WebhookRouteId, destinationId = route.Id }, route);
	}

	private static async Task<IResult> GetWebhookDestination(Guid routeId, Guid destinationId, IWebhookDestinationService service)
	{
		var route = await service.Get(routeId, destinationId);
		return Results.Ok(route);
	}

	private static async Task<IResult> ListWebhookDestinations(Guid routeId, IWebhookDestinationService service)
	{
		var routes = await service.List(routeId);
		return Results.Ok(routes);
	}
	
	private static async Task<IResult> DeleteWebhookDestination(Guid routeId, Guid destinationId, IWebhookDestinationService service)
	{
		await service.Delete(routeId, destinationId);
		return Results.NoContent();
	}
}