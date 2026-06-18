using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Infrastructure.Messaging;
using WebhookGateway.API.Persistence;

namespace WebhookGateway.Worker;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);
		
		builder.Services.AddHostedService<Worker>();
		builder.Services.AddSingleton(TimeProvider.System);
		builder.Services.AddHttpClient();
		
		builder.Services.AddDbContext<AppDbContext>(opts
			=> opts.UseNpgsql(builder.Configuration.GetConnectionString("Npgsql")));
		
		builder.Services.AddScoped<IWebhookDeliveryDispatcher, WebhookDeliveryDispatcher>();
		
		builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
		builder.Services.AddOptions<RabbitMqOptions>()
			.BindConfiguration(RabbitMqOptions.SectionName);

		var host = builder.Build();
		host.Run();
	}
}