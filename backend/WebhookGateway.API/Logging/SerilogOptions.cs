namespace WebhookGateway.API.Logging;

public sealed class SerilogOptions
{
	public const string SectionName = "Serilog";
	public const string MicrosoftCategory = "Microsoft";
	public const string MicrosoftAspNetCoreCategory = "Microsoft.AspNetCore";
	public const string MicrosoftHostingLifetimeCategory = "Microsoft.Hosting.Lifetime";

	public string ServiceName { get; init; } = string.Empty;
	public string LokiUrl { get; init; } = "http://localhost:3100";
	public string MinimumLevel { get; init; } = "Information";
	public Dictionary<string, string> Overrides { get; init; } = new()
	{
		[MicrosoftCategory] = "Warning",
		[MicrosoftAspNetCoreCategory] = "Warning",
		[MicrosoftHostingLifetimeCategory] = "Information"
	};
}
