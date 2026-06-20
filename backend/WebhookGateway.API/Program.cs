using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using WebhookGateway.API.Api.Middleware;
using WebhookGateway.API.Application.Sources;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Endpoints;
using WebhookGateway.API.Infrastructure.Messaging;
using WebhookGateway.API.Infrastructure.Webhooks;
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

		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.ConfigureHttpJsonOptions(options
			=> options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

		builder.Services.AddScoped<IWebhookRouteService, WebhookRouteService>();
		builder.Services.AddScoped<IWebhookDestinationService, WebhookDestinationService>();

		builder.Services.AddHostedService<RabbitMqInitializationHostedService>();
		builder.Services.AddSingleton<IRabbitMqInitializer, RabbitMqInitializer>();

		builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
		builder.Services.AddOptions<RabbitMqOptions>()
			.BindConfiguration(RabbitMqOptions.SectionName);
		
		builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
		builder.Services.AddScoped<IWebhookIngestor, WebhookIngestor>();

		builder.Services.AddOptions<WebhookSecrets>()
			.BindConfiguration(WebhookSecrets.SectionName);
		
		builder.Services.AddSingleton<IWebhookSourceSecretProvider, WebhookSourceSecretProvider>();
		
		builder.Services.AddSingleton<WebhookSourceResolver>();
		builder.Services.AddSingleton<IWebhookSourceHandler, TestWebhookSourceHandler>();
		builder.Services.AddSingleton<IWebhookSourceHandler, GithubWebhookSourceHandler>();


		var app = builder.Build();
		
		app.UseMiddleware<ExceptionHandlingMiddleware>();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();

		app.MapGet("/", () => "success");

		app.MapWebhookEndpoints();
		
		app.Run();
	}
}