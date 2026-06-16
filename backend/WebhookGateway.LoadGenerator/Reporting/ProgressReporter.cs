using System.Diagnostics;

namespace WebhookGateway.LoadGenerator;

internal sealed class ProgressReporter : IDisposable
{
    private readonly LoadTestResult _result;
    private readonly TimeSpan _interval;
    private readonly CancellationTokenSource _complete = new();
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private long _lastCompleted;
    private TimeSpan _lastElapsed;

    public ProgressReporter(LoadTestResult result, TimeSpan interval)
    {
        _result = result;
        _interval = interval;
    }

    public async Task RunUntilComplete()
    {
        try
        {
            using var timer = new PeriodicTimer(_interval);

            while (await timer.WaitForNextTickAsync(_complete.Token))
                PrintProgress();
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Complete()
    {
        _complete.Cancel();
    }

    public void Dispose()
    {
        _complete.Dispose();
    }

    private void PrintProgress()
    {
        var completed = _result.Completed;
        var attempted = _result.Attempted;
        var success = _result.Success;
        var failure = _result.Failure;
        var elapsed = _stopwatch.Elapsed;
        var averageRps = elapsed.TotalSeconds > 0
            ? completed / elapsed.TotalSeconds
            : 0;
        var intervalSeconds = (elapsed - _lastElapsed).TotalSeconds;
        var intervalRps = intervalSeconds > 0
            ? (completed - _lastCompleted) / intervalSeconds
            : 0;

        _lastCompleted = completed;
        _lastElapsed = elapsed;

        Console.WriteLine(FormattableString.Invariant(
            $"progress elapsed={FormatDuration(elapsed)} attempted={attempted:N0} completed={completed:N0} success={success:N0} failure={failure:N0} avg-rps={averageRps:N2} interval-rps={intervalRps:N2}"));
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"m\:ss");
    }
}
