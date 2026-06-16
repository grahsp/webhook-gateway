using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace WebhookGateway.LoadGenerator;

internal sealed class LoadTestRunner
{
    private readonly HttpClient _client;
    private readonly LoadGeneratorOptions _options;

    public LoadTestRunner(HttpClient client, LoadGeneratorOptions options)
    {
        _client = client;
        _options = options;
    }

    public async Task<LoadTestResult> Run()
    {
        var result = new LoadTestResult();
        var payload = PayloadGenerator.Generate(_options.PayloadSizeKb);
        using var progress = new ProgressReporter(result, _options.ProgressInterval);

        var stopwatch = Stopwatch.StartNew();
        var workers = Enumerable
            .Range(0, _options.Concurrency)
            .Select(workerId => RunWorker(workerId, payload, result, stopwatch))
            .ToArray();

        var progressTask = progress.RunUntilComplete();
        var workerResults = await Task.WhenAll(workers);

        stopwatch.Stop();
        result.Elapsed = stopwatch.Elapsed;
        progress.Complete();
        await progressTask;

        foreach (var workerResult in workerResults)
            result.Merge(workerResult);

        return result;
    }

    private async Task<WorkerResult> RunWorker(
        int workerId,
        string payload,
        LoadTestResult result,
        Stopwatch stopwatch)
    {
        var workerResult = new WorkerResult();

        while (TryReserveRequest(result, stopwatch, out var requestNumber))
        {
            await ExecuteRequest(workerId, requestNumber, payload, result, workerResult);
        }

        return workerResult;
    }

    private bool TryReserveRequest(
        LoadTestResult result,
        Stopwatch stopwatch,
        out long requestNumber)
    {
        if (_options.Duration is { } duration)
        {
            if (stopwatch.Elapsed >= duration)
            {
                requestNumber = 0;
                return false;
            }

            requestNumber = result.ReserveRequest();
            return true;
        }

        return result.TryReserveRequest(_options.Requests!.Value, out requestNumber);
    }

    private async Task ExecuteRequest(
        int workerId,
        long requestNumber,
        string payload,
        LoadTestResult result,
        WorkerResult workerResult)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var request = CreateRequest(workerId, requestNumber, payload);
            using var timeout = new CancellationTokenSource(_options.Timeout);
            using var response = await _client.SendAsync(request, timeout.Token);

            var success = workerResult.Record(
                stopwatch.ElapsedMilliseconds,
                response.StatusCode,
                exceptionType: null);

            result.RecordCompletion(success);
        }
        catch (Exception exception)
        {
            var exceptionType = exception is OperationCanceledException
                ? nameof(TimeoutException)
                : exception.GetType().Name;

            var success = workerResult.Record(
                stopwatch.ElapsedMilliseconds,
                statusCode: null,
                exceptionType);

            result.RecordCompletion(success);
        }
    }

    private HttpRequestMessage CreateRequest(
        int workerId,
        long requestNumber,
        string payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _options.Url);

        request.Headers.Add("X-Test-Delivery", CreateDeliveryId(workerId, requestNumber));
        request.Headers.Add("X-Test-Event", _options.EventType);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        return request;
    }

    private static string CreateDeliveryId(int workerId, long requestNumber)
    {
        if (requestNumber > 0)
            return $"{requestNumber.ToString(CultureInfo.InvariantCulture)}-{Guid.NewGuid():N}";

        return $"{workerId.ToString(CultureInfo.InvariantCulture)}-{Guid.NewGuid():N}";
    }
}
