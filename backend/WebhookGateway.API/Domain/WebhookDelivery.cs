using System.Diagnostics.CodeAnalysis;

namespace WebhookGateway.API.Domain;

public sealed class WebhookDelivery
{
	public Guid Id { get; private set; }
	
	public Guid WebhookEventId { get; private set; }
	public WebhookEvent WebhookEvent { get; private set; } = null!;
	
	public Guid WebhookDestinationId { get; private set; }
	public WebhookDestination WebhookDestination { get; private set; } = null!;
	
	private readonly List<WebhookDeliveryAttempt> _attempts = [];
	public IReadOnlyCollection<WebhookDeliveryAttempt> Attempts => _attempts;
	private int AttemptCount => _attempts.Count;
	
	public DeliveryStatus Status { get; private set; } = DeliveryStatus.Pending;
	private bool IsTerminal => Status is DeliveryStatus.Succeeded or DeliveryStatus.Failed;
	
	public DateTimeOffset CreatedAt { get; private set; }
	
	private WebhookDelivery() {}

	internal WebhookDelivery(Guid eventId, Guid destinationId, DateTimeOffset now)
	{
		WebhookEventId = eventId;
		WebhookDestinationId = destinationId;
		CreatedAt = now;
	}

	public WebhookDeliveryAttempt StartAttempt(DateTimeOffset now)
	{
		if (IsTerminal)
			throw new InvalidOperationException("Delivery already succeeded");
		
		if (TryGetAttemptInProgress(out _))
			throw new InvalidOperationException("Delivery already in progress");
		
		var attempt = WebhookDeliveryAttempt.StartAttempt(Id, _attempts.Count + 1, now);
		_attempts.Add(attempt);

		Status = DeliveryStatus.InProgress;
		return attempt;
	}

	public void MarkSucceeded(int statusCode, DateTimeOffset now)
	{
		if (IsTerminal)
			throw new InvalidOperationException("Delivery not in progress");
		
		if (!TryGetAttemptInProgress(out var attempt))
			throw new InvalidOperationException("No attempt in progress");
		
		attempt.RecordSuccess(statusCode, now);
		Status = DeliveryStatus.Succeeded;
	}
	
	public void MarkAttemptFailed(int? statusCode, string? errorMessage, DateTimeOffset now)
	{
		if (IsTerminal)
			throw new InvalidOperationException("Delivery not in progress");
		
		if (!TryGetAttemptInProgress(out var attempt))
			throw new InvalidOperationException("No attempt in progress");
		
		attempt.RecordFailure(statusCode, errorMessage, now);
	}
	
	public void MarkFailed()
	{
		if (IsTerminal)
			throw new InvalidOperationException("Delivery not in progress");
		
		Status = DeliveryStatus.Failed;
	}

	private bool TryGetAttemptInProgress([NotNullWhen(true)] out WebhookDeliveryAttempt? attempt)
	{
		attempt = _attempts.SingleOrDefault(x => x.Status == DeliveryAttemptStatus.InProgress);
		return attempt != null;
	}
}