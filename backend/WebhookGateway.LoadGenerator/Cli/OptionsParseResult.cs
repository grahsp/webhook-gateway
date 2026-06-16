namespace WebhookGateway.LoadGenerator;

internal sealed record OptionsParseResult(
    LoadGeneratorOptions? Options,
    string? Error,
    bool ShowHelp)
{
    public static OptionsParseResult Valid(LoadGeneratorOptions options) => new(options, null, ShowHelp: false);
    public static OptionsParseResult Invalid(string error) => new(null, error, ShowHelp: false);
    public static OptionsParseResult Help() => new(null, null, ShowHelp: true);
}
