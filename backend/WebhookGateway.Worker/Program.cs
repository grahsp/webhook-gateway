using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Infrastructure.Messaging;
using WebhookGateway.API.Infrastructure.Metrics;
using WebhookGateway.API.Logging;
using WebhookGateway.API.Infrastructure.Webhooks;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.Worker;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		
		builder.Services.AddWebhookGatewaySerilog(
			builder.Configuration,
			builder.Environment,
			ServiceNames.Worker);
		
		builder.Services.AddHostedService<Worker>();
		builder.Services.AddSingleton(TimeProvider.System);
		builder.Services.AddHttpClient();
		
		builder.Services.AddDbContext<AppDbContext>(opts
			=> opts.UseNpgsql(builder.Configuration.GetConnectionString("Npgsql")));

		builder.Services.AddSingleton<WorkerMetrics>();
		
		builder.Services
			.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(
				SerilogConfiguration.GetServiceName(builder.Configuration, ServiceNames.Worker)))
			.WithMetrics(metrics =>
			{
				metrics
					.AddMeter(WorkerMetrics.MeterName)
					.AddPrometheusExporter();
			});
		
		builder.Services.AddScoped<IWebhookDeliveryFailureClassifier, WebhookDeliveryFailureClassifier>();
		builder.Services.AddScoped<IWebhookDeliveryRetryPolicy, WebhookDeliveryRetryPolicy>();
		builder.Services.AddScoped<IWebhookBatchDeliveryDispatcher, WebhookBatchDeliveryDispatcher>();
		
		builder.Services.AddHostedService<RabbitMqInitializationHostedService>();
		builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
		builder.Services.AddSingleton<IRabbitMqInitializer, RabbitMqInitializer>();
		
		builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
		builder.Services.AddOptions<RabbitMqOptions>()
			.BindConfiguration(RabbitMqOptions.SectionName);

		var app = builder.Build();

		app.MapPrometheusScrapingEndpoint();
		
		app.Run();
	}
}
