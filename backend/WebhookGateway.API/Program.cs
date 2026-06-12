using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Application.Sources;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Endpoints;
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
		
		builder.Services.AddScoped<IWebhookIngestor, WebhookIngestor>();
		
		builder.Services.AddSingleton<WebhookSourceResolver>();
		builder.Services.AddSingleton<IWebhookSourceHandler, GithubWebhookSourceHandler>();


		var app = builder.Build();

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