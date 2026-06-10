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

			var webhookEvent = new WebhookEvent(
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

public sealed class WebhookEvent(
	Guid Id,
	string Provider,
	string Payload,
	Dictionary<string, string> Headers,
	DateTimeOffset ReceivedAt
);