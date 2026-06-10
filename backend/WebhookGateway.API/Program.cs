using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Persistence;

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

		builder.Services.AddDbContext<AppDbContext>(opts
			=> opts.UseNpgsql(builder.Configuration.GetConnectionString("Npgsql")));
		
		builder.Services.AddSingleton<IWebhookProvider, GithubWebhookProvider>();
		builder.Services.AddSingleton<WebhookProviderResolver>();


		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();

		app.MapGet("/", () => "success");

		app.MapPost("/webhooks/{providerName}", async (string providerName, WebhookProviderResolver resolver, AppDbContext db, HttpRequest request) =>
		{
			var provider = resolver.Resolve(providerName);
			
			using var reader = new StreamReader(request.Body);

			var payload = await reader.ReadToEndAsync();
			var metadata = provider.ExtractMetadata(request);

			var webhookEvent = WebhookEvent.New(
				Guid.NewGuid(),
				metadata,
				payload,
				DateTimeOffset.UtcNow);
			
			db.WebhookEvents.Add(webhookEvent);
			await db.SaveChangesAsync();

			return Results.Ok();
		});
		
		app.Run();
	}
}

public sealed class WebhookEvent{
	public Guid Id { get; private set; }
	public WebhookEventMetadata Metadata { get; private set; } = null!;
	public string Payload { get; private set; } = null!;
	public DateTimeOffset ReceivedAt { get; private set; }
	
	private WebhookEvent() { }

	private WebhookEvent(Guid id, WebhookEventMetadata metadata, string payload, DateTimeOffset receivedAt)
	{
		Id = id;
		Metadata = metadata;
		Payload = payload;
		ReceivedAt = receivedAt;
	}
	
	public static WebhookEvent New(Guid id, WebhookEventMetadata metadata, string payload, DateTimeOffset receivedAt)
		=> new WebhookEvent(id, metadata, payload, receivedAt);
}

public sealed record WebhookEventMetadata(string Provider, string? DeliveryId, string? EventType);

public interface IWebhookProvider
{
	string Name { get; }
	WebhookEventMetadata ExtractMetadata(HttpRequest request);
}

public class GithubWebhookProvider : IWebhookProvider
{
	public string Name => "github";
	
	public WebhookEventMetadata ExtractMetadata(HttpRequest request)
	{
		request.Headers.TryGetValue("X-GitHub-Delivery", out var delivery);
		request.Headers.TryGetValue("X-GitHub-Event", out var type);
		
		return new WebhookEventMetadata(Name, delivery, type);
	}
}

public sealed class WebhookProviderResolver
{
	private readonly IReadOnlyDictionary<string, IWebhookProvider> _providers;

	public WebhookProviderResolver(IEnumerable<IWebhookProvider> providers)
	{
		_providers = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
	}

	public IWebhookProvider Resolve(string provider)
		=> _providers.GetValueOrDefault(provider) ?? throw new InvalidOperationException($"No provider found for '{provider}'");
}