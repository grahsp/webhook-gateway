namespace WebhookGateway.API.Infrastructure.Webhooks;

public enum DeliveryResultType
{
	Success,
	TransientFailure,
	PermanentFailure
}

public sealed record DeliveryDispatchResult
{
	public Guid DeliveryId { get; }
	public DeliveryResultType Type { get; }
	public int? StatusCode { get; }
	public string? ErrorMessage { get; }
	
	private DeliveryDispatchResult(Guid deliveryId, DeliveryResultType type, int? statusCode, string? errorMessage)
	{
		DeliveryId = deliveryId;
		Type = type;
		StatusCode = statusCode;
		ErrorMessage = errorMessage;
	}
	
	public static DeliveryDispatchResult Success(Guid deliveryId, int? statusCode)
		=> new DeliveryDispatchResult(deliveryId, DeliveryResultType.Success, statusCode, null);
	
	public static DeliveryDispatchResult Transient(Guid deliveryId, int? statusCode, string? errorMessage) =>
		new DeliveryDispatchResult(deliveryId, DeliveryResultType.TransientFailure, statusCode, errorMessage);
	
	public static DeliveryDispatchResult Permanent(Guid deliveryId, int? statusCode, string? errorMessage) =>
		new DeliveryDispatchResult(deliveryId, DeliveryResultType.PermanentFailure, statusCode, errorMessage);
}

public interface IWebhookDeliveryFailureClassifier
{
	DeliveryDispatchResult Classify(Guid deliveryId, HttpResponseMessage response);
	DeliveryDispatchResult Classify(Guid deliveryId, Exception exception);
}

public sealed class WebhookDeliveryFailureClassifier : IWebhookDeliveryFailureClassifier
{
	public DeliveryDispatchResult Classify(Guid deliveryId, HttpResponseMessage response)
	{
		var statusCode = (int)response.StatusCode;

		if (response.IsSuccessStatusCode)
			return DeliveryDispatchResult.Success(deliveryId, statusCode);

		return statusCode switch
		{
			408 => DeliveryDispatchResult.Transient(deliveryId, statusCode, response.ReasonPhrase),
			404 => DeliveryDispatchResult.Transient(deliveryId, statusCode, response.ReasonPhrase),
			409 => DeliveryDispatchResult.Transient(deliveryId, statusCode, response.ReasonPhrase),
			425 => DeliveryDispatchResult.Transient(deliveryId, statusCode, response.ReasonPhrase),
			429 => DeliveryDispatchResult.Transient(deliveryId, statusCode, response.ReasonPhrase),

			>= 500 and <= 599 => DeliveryDispatchResult.Transient(deliveryId, statusCode, response.ReasonPhrase),

			400 => DeliveryDispatchResult.Permanent(deliveryId, statusCode, response.ReasonPhrase),
			401 => DeliveryDispatchResult.Permanent(deliveryId, statusCode, response.ReasonPhrase),
			403 => DeliveryDispatchResult.Permanent(deliveryId, statusCode, response.ReasonPhrase),
			410 => DeliveryDispatchResult.Permanent(deliveryId, statusCode, response.ReasonPhrase),
			413 => DeliveryDispatchResult.Permanent(deliveryId, statusCode, response.ReasonPhrase),
			415 => DeliveryDispatchResult.Permanent(deliveryId, statusCode, response.ReasonPhrase),
			422 => DeliveryDispatchResult.Permanent(deliveryId, statusCode, response.ReasonPhrase),

			_ => DeliveryDispatchResult.Transient(deliveryId, statusCode, response.ReasonPhrase)
		};
	}

	public DeliveryDispatchResult Classify(Guid deliveryId, Exception ex)
	{
		return DeliveryDispatchResult.Transient(deliveryId, null, ex.Message);
	}
}