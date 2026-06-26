using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Application.Webhooks;
using WebhookGateway.API.Infrastructure.Messaging;
using WebhookGateway.API.Infrastructure.Webhooks;
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
		
		builder.Services.AddScoped<IWebhookDeliveryFailureClassifier, WebhookDeliveryFailureClassifier>();
		builder.Services.AddScoped<IWebhookDeliveryRetryPolicy, WebhookDeliveryRetryPolicy>();
		builder.Services.AddScoped<IWebhookBatchDeliveryDispatcher, WebhookBatchDeliveryDispatcher>();
		
		builder.Services.AddHostedService<RabbitMqInitializationHostedService>();
		builder.Services.AddSingleton<IRabbitMqInitializer, RabbitMqInitializer>();
		
		builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
		builder.Services.AddOptions<RabbitMqOptions>()
			.BindConfiguration(RabbitMqOptions.SectionName);

		var host = builder.Build();
		host.Run();
	}
}