using WebhookGateway.API.Api.Extensions;
using WebhookGateway.API.Application.Webhooks;

namespace WebhookGateway.API.Endpoints;

public static class WebhookEndpoints
{
	public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapPost("/webhooks/{provider}", ReceiveWebhook);

		return app;
	}

	private static async Task<IResult> ReceiveWebhook(string provider, HttpRequest request, IWebhookIngestor ingestor)
	{
		var data = await request.ExtractWebhookRequest();
		await ingestor.Ingest(provider, data);

		return Results.Ok();
	}
}