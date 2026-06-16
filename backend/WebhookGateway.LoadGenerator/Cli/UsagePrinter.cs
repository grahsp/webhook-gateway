namespace WebhookGateway.LoadGenerator;

internal static class UsagePrinter
{
    public static void Print(TextWriter writer)
    {
        writer.WriteLine("Usage:");
        writer.WriteLine("  WebhookGateway.LoadGenerator --url <url> [options]");
        writer.WriteLine();
        writer.WriteLine("Required:");
        writer.WriteLine("  --url <url>                  Target http or https webhook/API URL.");
        writer.WriteLine();
        writer.WriteLine("Options:");
        writer.WriteLine("  --requests <n>               Total requests to send. Default: 1000.");
        writer.WriteLine("  --duration <timespan>         Run for a duration instead of a request count.");
        writer.WriteLine("  --concurrency <n>             Number of fixed worker tasks. Default: 20.");
        writer.WriteLine("  --payload-kb <n>              Approximate generated JSON payload size. Default: 10.");
        writer.WriteLine("  --timeout <timespan>          Per-request timeout. Default: 10s.");
        writer.WriteLine("  --progress-interval <span>    Progress print interval. Default: 1s.");
        writer.WriteLine("  --event-type <value>          X-Test-Event header value. Default: test.");
        writer.WriteLine("  --help                        Show this help.");
        writer.WriteLine();
        writer.WriteLine("Timespans accept 500ms, 10s, 2m, 1h, or TimeSpan values like 00:00:10.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  WebhookGateway.LoadGenerator --url http://localhost:5073/webhooks/test --requests 1000 --concurrency 20");
        writer.WriteLine("  WebhookGateway.LoadGenerator --url http://localhost:5073/webhooks/test --duration 30s --payload-kb 40");
    }
}
