namespace WebhookGateway.LoadGenerator;

internal static class SummaryPrinter
{
    public static void PrintFinal(LoadTestResult result)
    {
        var completed = result.Completed;
        var failureRate = completed > 0 ? result.Failure / (double)completed * 100 : 0;
        var rps = result.Elapsed.TotalSeconds > 0 ? completed / result.Elapsed.TotalSeconds : 0;
        var latencySummary = LatencySummary.From(result.Latencies.ToArray());

        Console.WriteLine();
        Console.WriteLine("Summary");
        Console.WriteLine(FormattableString.Invariant($"  Attempted:    {result.Attempted:N0}"));
        Console.WriteLine(FormattableString.Invariant($"  Completed:    {completed:N0}"));
        Console.WriteLine(FormattableString.Invariant($"  Success:      {result.Success:N0}"));
        Console.WriteLine(FormattableString.Invariant($"  Failure:      {result.Failure:N0}"));
        Console.WriteLine(FormattableString.Invariant($"  Failure rate: {failureRate:N2}%"));
        Console.WriteLine($"  Elapsed:      {result.Elapsed}");
        Console.WriteLine(FormattableString.Invariant($"  Average RPS:  {rps:N2}"));
        Console.WriteLine("  Latency:");
        Console.WriteLine($"    Min:    {latencySummary.Min} ms");
        Console.WriteLine(FormattableString.Invariant($"    Mean:   {latencySummary.Mean:N2} ms"));
        Console.WriteLine($"    P50:    {latencySummary.Median} ms");
        Console.WriteLine($"    P90:    {latencySummary.P90} ms");
        Console.WriteLine($"    P95:    {latencySummary.P95} ms");
        Console.WriteLine($"    P99:    {latencySummary.P99} ms");
        Console.WriteLine($"    Max:    {latencySummary.Max} ms");

        PrintBreakdown("HTTP status codes", result.StatusCodes.OrderBy(x => x.Key).Select(x => ($"{x.Key}", x.Value)));
        PrintBreakdown("Exception types", result.ExceptionTypes.OrderBy(x => x.Key).Select(x => (x.Key, x.Value)));
    }

    private static void PrintBreakdown(string label, IEnumerable<(string Key, long Value)> values)
    {
        Console.WriteLine($"  {label}:");

        var printed = false;

        foreach (var (key, value) in values)
        {
            Console.WriteLine(FormattableString.Invariant($"    {key}: {value:N0}"));
            printed = true;
        }

        if (!printed)
            Console.WriteLine("    none");
    }
}
