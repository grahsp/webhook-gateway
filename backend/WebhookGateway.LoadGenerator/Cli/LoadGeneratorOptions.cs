using System.Globalization;

namespace WebhookGateway.LoadGenerator;

internal sealed record LoadGeneratorOptions(
    Uri Url,
    int? Requests,
    TimeSpan? Duration,
    int Concurrency,
    int PayloadSizeKb,
    TimeSpan Timeout,
    TimeSpan ProgressInterval,
    string EventType)
{
    public static OptionsParseResult Parse(string[] args)
    {
        if (args.Length == 0)
            return OptionsParseResult.Invalid("Missing required --url option.");

        Uri? url = null;
        int? requests = null;
        var requestsSpecified = false;
        TimeSpan? duration = null;
        var concurrency = 20;
        var payloadKb = 10;
        var timeout = TimeSpan.FromSeconds(10);
        var progressInterval = TimeSpan.FromSeconds(1);
        var eventType = "test";

        for (var index = 0; index < args.Length; index++)
        {
            var option = args[index];

            if (option is "--help" or "-h")
                return OptionsParseResult.Help();

            if (!option.StartsWith("--", StringComparison.Ordinal))
                return OptionsParseResult.Invalid($"Unexpected argument '{option}'.");

            if (!TryReadValue(args, ref index, option, out var value, out var error))
                return OptionsParseResult.Invalid(error!);

            switch (option)
            {
                case "--url":
                    if (!Uri.TryCreate(value, UriKind.Absolute, out url) ||
                        url.Scheme is not ("http" or "https"))
                    {
                        return OptionsParseResult.Invalid("--url must be an absolute http or https URL.");
                    }

                    break;

                case "--requests":
                    if (!TryParsePositiveInt(value, option, out var parsedRequests, out error))
                        return OptionsParseResult.Invalid(error!);

                    requests = parsedRequests;
                    requestsSpecified = true;
                    break;

                case "--duration":
                    if (!TryParseDuration(value, option, out var parsedDuration, out error))
                        return OptionsParseResult.Invalid(error!);

                    duration = parsedDuration;
                    break;

                case "--concurrency":
                    if (!TryParsePositiveInt(value, option, out concurrency, out error))
                        return OptionsParseResult.Invalid(error!);

                    break;

                case "--payload-kb":
                    if (!TryParsePositiveInt(value, option, out payloadKb, out error))
                        return OptionsParseResult.Invalid(error!);

                    break;

                case "--timeout":
                    if (!TryParseDuration(value, option, out timeout, out error))
                        return OptionsParseResult.Invalid(error!);

                    break;

                case "--progress-interval":
                    if (!TryParseDuration(value, option, out progressInterval, out error))
                        return OptionsParseResult.Invalid(error!);

                    break;

                case "--event-type":
                    if (string.IsNullOrWhiteSpace(value))
                        return OptionsParseResult.Invalid("--event-type cannot be empty.");

                    eventType = value;
                    break;

                default:
                    return OptionsParseResult.Invalid($"Unknown option '{option}'.");
            }
        }

        if (url is null)
            return OptionsParseResult.Invalid("Missing required --url option.");

        if (duration is not null && requestsSpecified)
            return OptionsParseResult.Invalid("--duration and --requests are mutually exclusive.");

        requests ??= duration is null ? 1000 : null;

        return OptionsParseResult.Valid(new LoadGeneratorOptions(
            url,
            requests,
            duration,
            concurrency,
            payloadKb,
            timeout,
            progressInterval,
            eventType));
    }

    private static bool TryReadValue(
        string[] args,
        ref int index,
        string option,
        out string value,
        out string? error)
    {
        value = string.Empty;
        error = null;

        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            error = $"{option} requires a value.";
            return false;
        }

        value = args[++index];
        return true;
    }

    private static bool TryParsePositiveInt(
        string value,
        string option,
        out int parsed,
        out string? error)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out parsed) || parsed <= 0)
        {
            error = $"{option} must be a positive integer.";
            return false;
        }

        error = null;
        return true;
    }

    private static bool TryParseDuration(
        string value,
        string option,
        out TimeSpan parsed,
        out string? error)
    {
        if (TryParseDurationSuffix(value, out parsed) ||
            TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out parsed))
        {
            if (parsed > TimeSpan.Zero)
            {
                error = null;
                return true;
            }
        }

        error = $"{option} must be a positive duration, for example 500ms, 10s, 2m, or 00:00:10.";
        return false;
    }

    private static bool TryParseDurationSuffix(string value, out TimeSpan parsed)
    {
        parsed = TimeSpan.Zero;

        var units = new[]
        {
            ("ms", TimeSpan.FromMilliseconds(1)),
            ("s", TimeSpan.FromSeconds(1)),
            ("m", TimeSpan.FromMinutes(1)),
            ("h", TimeSpan.FromHours(1))
        };

        foreach (var (suffix, unit) in units)
        {
            if (!value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                continue;

            var number = value[..^suffix.Length];

            if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
                return false;

            parsed = unit * amount;
            return true;
        }

        return false;
    }
}
