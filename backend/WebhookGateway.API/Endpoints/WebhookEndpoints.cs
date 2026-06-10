using WebhookGateway.API.Domain;
using WebhookGateway.API.Persistence;
using WebhookGateway.API.Providers;

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

		using var reader = new StreamReader(request.Body);
		var payload = await reader.ReadToEndAsync();

		var metadata = provider.ExtractMetadata(request);

		var webhookEvent = WebhookEvent.New(
			Guid.NewGuid(),
			metadata,
			payload,
			DateTimeOffset.UtcNow);

		db.WebhookEvents.Add(webhookEvent);
		await db.SaveChangesAsync();

		return Results.Ok();
	}
}