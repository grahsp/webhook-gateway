using WebhookGateway.API.Api.Extensions;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Endpoints;

public static class WebhookEndpoints
{
	public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapPost("/webhooks/{sourceName}", ReceiveWebhook);

		return app;
	}

	private static async Task<IResult> ReceiveWebhook(string sourceName, HttpRequest request, IWebhookIngestor ingestor)
	{
		if (!Enum.TryParse<WebhookSource>(sourceName, true, out var source))
			return Results.BadRequest();
		
		var data = await request.ExtractWebhookRequest();
		await ingestor.Ingest(source, data);

		return Results.Ok();
	}
}