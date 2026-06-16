using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Api.Middleware;
using WebhookGateway.API.Application.Sources;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Endpoints;
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

		builder.Services.AddHttpClient();
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.ConfigureHttpJsonOptions(options
			=> options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

		builder.Services.AddScoped<IWebhookRouteService, WebhookRouteService>();
		builder.Services.AddScoped<IWebhookDestinationService, WebhookDestinationService>();
		builder.Services.AddScoped<IWebhookIngestor, WebhookIngestor>();

		builder.Services.AddScoped<IWebhookDeliveryDispatcher, WebhookDeliveryDispatcher>();

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