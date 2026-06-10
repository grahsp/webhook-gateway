using WebhookGateway.API.Api.Extensions;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Domain;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.API.Endpoints;

public static class WebhookEndpoints
{
	public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapPost("/webhooks/{providerName}", ReceiveWebhook);

		return app;
	}

	private static async Task<IResult> ReceiveWebhook(
		string providerName,
		HttpRequest request,
		WebhookProviderResolver resolver,
		AppDbContext db)
	{
		var provider = resolver.Resolve(providerName);
		
		var webhookRequest = await request.ExtractWebhookRequest();
		var metadata = provider.ExtractMetadata(webhookRequest);

		var webhookEvent = WebhookEvent.New(
			Guid.NewGuid(),
			metadata,
			webhookRequest.Payload,
			DateTimeOffset.UtcNow);

		db.WebhookEvents.Add(webhookEvent);
		await db.SaveChangesAsync();

		return Results.Ok();
	}
}