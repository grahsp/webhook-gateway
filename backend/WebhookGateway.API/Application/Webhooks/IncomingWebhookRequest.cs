namespace WebhookGateway.API.Application.Webhooks;

public sealed record IncomingWebhookRequest(
	string Payload,
	IReadOnlyDictionary<string, string> Headers
);