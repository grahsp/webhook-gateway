using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace WebhookGateway.API.Logging;

public static class SerilogConfiguration
{
	public static IServiceCollection AddWebhookGatewaySerilog(
		this IServiceCollection services,
		IConfiguration configuration,
		IHostEnvironment environment,
		string defaultServiceName)
	{
		var options = GetOptions(configuration, defaultServiceName);
		
		services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
			.MinimumLevel.Is(ParseLevel(options.MinimumLevel, LogEventLevel.Information))
			.MinimumLevel.Override(
				SerilogOptions.MicrosoftCategory,
				GetOverrideLevel(options, SerilogOptions.MicrosoftCategory, LogEventLevel.Warning))
			.MinimumLevel.Override(
				SerilogOptions.MicrosoftAspNetCoreCategory,
				GetOverrideLevel(options, SerilogOptions.MicrosoftAspNetCoreCategory, LogEventLevel.Warning))
			.MinimumLevel.Override(
				SerilogOptions.MicrosoftHostingLifetimeCategory,
				GetOverrideLevel(options, SerilogOptions.MicrosoftHostingLifetimeCategory, LogEventLevel.Information))
			.Enrich.FromLogContext()
			.WriteTo.GrafanaLoki(
				options.LokiUrl,
				[
					new LokiLabel { Key = "service_name", Value = options.ServiceName },
					new LokiLabel { Key = "environment", Value = environment.EnvironmentName }
				]));

		return services;
	}

	public static string GetServiceName(IConfiguration configuration, string defaultServiceName)
		=> GetOptions(configuration, defaultServiceName).ServiceName;

	private static SerilogOptions GetOptions(IConfiguration configuration, string defaultServiceName)
	{
		var options = configuration
			.GetSection(SerilogOptions.SectionName)
			.Get<SerilogOptions>() ?? new SerilogOptions();

		return string.IsNullOrWhiteSpace(options.ServiceName)
			? new SerilogOptions
			{
				ServiceName = defaultServiceName,
				LokiUrl = options.LokiUrl,
				MinimumLevel = options.MinimumLevel,
				Overrides = options.Overrides
			}
			: options;
	}

	private static LogEventLevel GetOverrideLevel(SerilogOptions options, string category, LogEventLevel fallback)
	{
		if (!options.Overrides.TryGetValue(category, out var configuredLevel))
			return fallback;

		return ParseLevel(configuredLevel, fallback);
	}
	
	private static LogEventLevel ParseLevel(string? configuredLevel, LogEventLevel fallback)
	{
		return Enum.TryParse(configuredLevel, ignoreCase: true, out LogEventLevel level)
			? level
			: fallback;
	}
}
