using WebhookGateway.API.Application.Webhooks;

namespace WebhookGateway.API.Api.Extensions;

public static class HttpRequestExtensions
{
	public static async Task<IncomingWebhookRequest> ExtractWebhookRequest(this HttpRequest request)
	{
		using var reader = new StreamReader(request.Body);
		var payload = await reader.ReadToEndAsync();
		
		var headers = request.Headers
			.ToDictionary(x => x.Key, x => x.Value.ToString());

		return new IncomingWebhookRequest(payload, headers);
	}
}