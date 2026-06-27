namespace WebhookGateway.API.Application.Webhooks;

public enum DeliveryAction
{
	Ack,
	Retry,
	DeadLetter
}