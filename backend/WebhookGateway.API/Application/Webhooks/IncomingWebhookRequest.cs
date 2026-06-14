using System.Text;

namespace WebhookGateway.API.Application.Webhooks;

public sealed class IncomingWebhookRequest(byte[] payloadBytes, Dictionary<string, string> headers)
{
	public byte[] PayloadBytes { get; } = payloadBytes;
	public IReadOnlyDictionary<string, string> Headers { get; } = headers;

	public string Payload => field ??= Encoding.UTF8.GetString(PayloadBytes);
}