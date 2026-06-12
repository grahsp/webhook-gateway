using WebhookGateway.API.Api.Extensions;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Application.Webhooks.Models;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Endpoints;

public static class WebhookEndpoints
{
	public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapPost("/webhooks/{routeId}", ReceiveWebhook);

		app.MapPost("/route", CreateWebhookRoute);
		app.MapGet("/route", ListWebhookRoutes);
		app.MapGet("/route/{id}", GetWebhookRoute)
			.WithName(nameof(GetWebhookRoute));
		app.MapDelete("/route", DeleteWebhookRoute);

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
		return Results.CreatedAtRoute(nameof(GetWebhookRoute), new { route.Id }, route);
	}

	private static async Task<IResult> GetWebhookRoute(Guid id, IWebhookRouteService service)
	{
		var route = await service.Get(id);
		return Results.Ok(route);
	}

	private static async Task<IResult> ListWebhookRoutes(IWebhookRouteService service)
	{
		var routes = await service.List();
		return Results.Ok(routes);
	}
	
	private static async Task<IResult> DeleteWebhookRoute(Guid id, IWebhookRouteService service)
	{
		await service.Delete(id);
		return Results.NoContent();
	}
}