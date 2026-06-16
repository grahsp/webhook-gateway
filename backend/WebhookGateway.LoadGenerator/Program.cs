namespace WebhookGateway.LoadGenerator;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var parseResult = LoadGeneratorOptions.Parse(args);

        if (parseResult.ShowHelp)
        {
            UsagePrinter.Print(Console.Out);
            return 0;
        }

        if (parseResult.Error is not null)
        {
            Console.Error.WriteLine(parseResult.Error);
            Console.Error.WriteLine();
            UsagePrinter.Print(Console.Error);
            return 1;
        }

        using var handler = new SocketsHttpHandler
        {
            MaxConnectionsPerServer = parseResult.Options!.Concurrency,
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            PooledConnectionLifetime = TimeSpan.FromMinutes(10)
        };

        using var client = new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        var runner = new LoadTestRunner(client, parseResult.Options);
        var result = await runner.Run();

        SummaryPrinter.PrintFinal(result);
        return 0;
    }
}
