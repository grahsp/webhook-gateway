using Microsoft.Extensions.Options;
using WebhookGateway.API.Logging;

namespace WebhookGateway.API.Infrastructure.Messaging;

public sealed class RabbitMqInitializationHostedService(
	IRabbitMqInitializer initializer,
	IOptions<RabbitMqOptions> options,
	ILogger<RabbitMqInitializationHostedService> logger)
	: IHostedService
{
	private readonly RabbitMqOptions _options = options.Value;
	
	public async Task StartAsync(CancellationToken ct)
	{
		try
		{
			logger.RabbitMqInitializationStarted(
				_options.DeliveryQueue,
				_options.RetryQueue,
				_options.DeadLetterQueue,
				_options.RetryDelaySeconds);
			await initializer.InitializeAsync(ct);
			
			logger.RabbitMqInitializationCompleted(
				_options.DeliveryQueue,
				_options.RetryQueue,
				_options.DeadLetterQueue);
		}
		catch (Exception ex)
		{
			logger.RabbitMqInitializationFailed(
				ex,
				_options.DeliveryQueue,
				_options.RetryQueue,
				_options.DeadLetterQueue);
			throw;
		}
	}

	public Task StopAsync(CancellationToken ct)
		=> Task.CompletedTask;
}
