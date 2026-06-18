namespace WebhookGateway.API.Infrastructure.Messaging;

public sealed class RabbitMqInitializationHostedService(
	IRabbitMqInitializer initializer,
	ILogger<RabbitMqInitializationHostedService> logger)
	: IHostedService
{
	public async Task StartAsync(CancellationToken ct)
	{
		try
		{
			logger.LogInformation("Initializing RabbitMQ..");
			await initializer.InitializeAsync(ct);
			
			logger.LogInformation("RabbitMQ initialization completed");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to initialize RabbitMQ");
			throw;
		}
	}

	public Task StopAsync(CancellationToken ct)
		=> Task.CompletedTask;
}