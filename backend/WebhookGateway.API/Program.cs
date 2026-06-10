using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Endpoints;
using WebhookGateway.API.Persistence;
using WebhookGateway.API.Providers;

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

		app.MapWebhookEndpoints();
		
		app.Run();
	}
}