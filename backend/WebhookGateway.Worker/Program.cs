using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using WebhookGateway.API.Application.Webhooks;
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
		
		builder.Services.AddSingleton<IConnection>(_ =>
		{
			var factory = new ConnectionFactory
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};

			return factory.CreateConnectionAsync()
				.GetAwaiter()
				.GetResult();
		});

		var host = builder.Build();
		host.Run();
	}
}