namespace WebhookGateway.LoadGenerator;

internal sealed record LatencySummary(
    long Min,
    double Mean,
    long Median,
    long P90,
    long P95,
    long P99,
    long Max)
{
    public static LatencySummary From(long[] values)
    {
        if (values.Length == 0)
            return new LatencySummary(0, 0, 0, 0, 0, 0, 0);

        Array.Sort(values);

        return new LatencySummary(
            values[0],
            values.Average(),
            Percentile(values, 0.50),
            Percentile(values, 0.90),
            Percentile(values, 0.95),
            Percentile(values, 0.99),
            values[^1]);
    }

    public override string ToString()
    {
        return FormattableString.Invariant($"min={Min}ms mean={Mean:N2}ms median={Median}ms p95={P95}ms max={Max}ms");
    }

    private static long Percentile(long[] values, double percentile)
    {
        var index = (int)Math.Ceiling(values.Length * percentile) - 1;
        return values[Math.Clamp(index, 0, values.Length - 1)];
    }
}
