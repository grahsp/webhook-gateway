namespace WebhookGateway.API.Domain;

public sealed class WebhookDeliveryAttempt
{
	public Guid Id { get; private set; }
	public Guid WebhookDeliveryId { get; private set; }

	public DeliveryAttemptStatus Status { get; private set; } = DeliveryAttemptStatus.InProgress;
	public int AttemptNumber { get; private set; }
	public int? StatusCode { get; private set; }
	public string? ErrorMessage { get; private set; }
	
	public DateTimeOffset StartedAt { get; private set; }
	public DateTimeOffset? FinishedAt { get; private set; }

	private bool IsCompleted => Status != DeliveryAttemptStatus.InProgress;
	
	private WebhookDeliveryAttempt() {}

	private WebhookDeliveryAttempt(Guid deliveryId, int attemptNumber, DateTimeOffset now)
	{
		Id = Guid.NewGuid();
		WebhookDeliveryId = deliveryId;
		AttemptNumber = attemptNumber;
		StartedAt = now;
	}
	
	internal static WebhookDeliveryAttempt StartAttempt(Guid deliveryId, int attemptNumber, DateTimeOffset now)
		=> new WebhookDeliveryAttempt(deliveryId, attemptNumber, now);

	internal void RecordSuccess(int statusCode, DateTimeOffset finishedAt)
	{
		if (IsCompleted)
			throw new InvalidOperationException("Attempt already recorded");
		
		Status = DeliveryAttemptStatus.Succeeded;
		
		StatusCode = statusCode;
		FinishedAt = finishedAt;
	}

	internal void RecordFailure(int? statusCode, string? errorMessage, DateTimeOffset finishedAt)
	{
		if (IsCompleted)
			throw new InvalidOperationException("Attempt already recorded");
		
		Status = DeliveryAttemptStatus.Failed;
		
		StatusCode = statusCode;
		ErrorMessage = errorMessage;
		FinishedAt = finishedAt;
	}
}