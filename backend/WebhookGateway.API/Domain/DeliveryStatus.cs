namespace WebhookGateway.API.Domain;

public enum DeliveryStatus
{
	Pending = 0,
	InProgress = 1,
	RetryPending = 2,
	Succeeded = 3,
	Failed = 4,
}