using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebhookGateway.API.Infrastructure.Metrics;

namespace WebhookGateway.API.Api.Extensions;

public static class HealthCheckEndpointExtensions
{
	public const string LiveTag = "live";
	public const string ReadyTag = "ready";

	public static IServiceCollection AddWebhookGatewayHealthChecks(this IServiceCollection services)
	{
		services.AddSingleton<HealthMetrics>();
		services.AddSingleton<IHealthCheckPublisher>(sp => sp.GetRequiredService<HealthMetrics>());
		services.Configure<HealthCheckPublisherOptions>(options =>
		{
			options.Delay = TimeSpan.FromSeconds(5);
			options.Period = TimeSpan.FromSeconds(10);
			options.Predicate = registration => registration.Tags.Contains(ReadyTag);
		});

		services
			.AddHealthChecks()
			.AddCheck("self", () => HealthCheckResult.Healthy(), tags: [LiveTag])
			.AddCheck<Infrastructure.Health.PostgresHealthCheck>("postgres", tags: [ReadyTag])
			.AddCheck<Infrastructure.Health.RabbitMqHealthCheck>("rabbitmq", tags: [ReadyTag]);

		return services;
	}

	public static IEndpointRouteBuilder MapWebhookGatewayHealthChecks(this IEndpointRouteBuilder app)
	{
		var liveOptions = CreateOptions(registration => registration.Tags.Contains(LiveTag));
		var readyOptions = CreateOptions(registration => registration.Tags.Contains(ReadyTag));

		app.MapHealthChecks("/", liveOptions);
		app.MapHealthChecks("/health/live", liveOptions);
		app.MapHealthChecks("/health/ready", readyOptions);

		return app;
	}

	private static HealthCheckOptions CreateOptions(Func<HealthCheckRegistration, bool> predicate)
		=> new()
		{
			Predicate = predicate,
			ResponseWriter = WriteHealthResponse
		};

	private static async Task WriteHealthResponse(HttpContext context, HealthReport report)
	{
		context.Response.ContentType = "application/json";

		var response = new
		{
			status = report.Status.ToString(),
			totalDurationMs = report.TotalDuration.TotalMilliseconds,
			checks = report.Entries.ToDictionary(
				entry => entry.Key,
				entry => new
				{
					status = entry.Value.Status.ToString(),
					description = entry.Value.Description,
					durationMs = entry.Value.Duration.TotalMilliseconds,
					error = entry.Value.Exception?.Message,
					tags = entry.Value.Tags
				})
		};

		await JsonSerializer.SerializeAsync(
			context.Response.Body,
			response,
			new JsonSerializerOptions(JsonSerializerDefaults.Web));
	}
}
