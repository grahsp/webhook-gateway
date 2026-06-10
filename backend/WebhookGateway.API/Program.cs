namespace WebhookGateway.API;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		if (builder.Environment.IsDevelopment())
		{
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
		}


		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();

		app.MapGet("/", () => "success");

		app.MapPost("/webhooks/{provider}", async (string provider, HttpRequest request) =>
		{
			using var reader = new StreamReader(request.Body);

			var payload = await reader.ReadToEndAsync();

			var webhookEvent = WebhookEvent.New(
				Guid.NewGuid(),
				provider,
				payload,
				request.Headers.ToDictionary(
					h => h.Key,
					h => h.Value.ToString()),
				DateTimeOffset.UtcNow);

			return Results.Ok();
		});
		
		app.Run();
	}
}

public sealed class WebhookEvent{
	public Guid Id { get; }
	public string Provider { get; }
	public string Payload { get; }
	public Dictionary<string, string> Headers { get; }
	public DateTimeOffset ReceivedAt { get; }

	private WebhookEvent(Guid id, string provider, string payload, Dictionary<string, string> headers, DateTimeOffset receivedAt)
	{
		Id = id;
		Provider = provider;
		Payload = payload;
		Headers = headers;
		ReceivedAt = receivedAt;
	}
	
	public static WebhookEvent New(Guid id, string provider, string payload, Dictionary<string, string> headers, DateTimeOffset receivedAt) =>
		new WebhookEvent(id, provider, payload, headers, receivedAt);
}