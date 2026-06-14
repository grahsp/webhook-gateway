using WebhookGateway.API.Application.Webhooks;

namespace WebhookGateway.API.Api.Extensions;

public static class HttpRequestExtensions
{
	public static async Task<IncomingWebhookRequest> ExtractWebhookRequest(this HttpRequest request)
	{
		using var stream = new MemoryStream();
		await request.Body.CopyToAsync(stream);
		
		var headers = request.Headers.ToDictionary(
				x => x.Key,
				x => x.Value.ToString(),
				StringComparer.OrdinalIgnoreCase);

		return new IncomingWebhookRequest(stream.ToArray(), headers);
	}
}