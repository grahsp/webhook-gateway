namespace WebhookGateway.LoadGenerator;

internal sealed class LoadTestResult
{
    private readonly List<long> _latencies = [];
    private readonly Dictionary<int, long> _statusCodes = [];
    private readonly Dictionary<string, long> _exceptionTypes = [];

    private long _attempted;
    private long _completed;
    private long _success;
    private long _failure;

    public TimeSpan Elapsed { get; set; }
    public long Attempted => Volatile.Read(ref _attempted);
    public long Completed => Volatile.Read(ref _completed);
    public long Success => Volatile.Read(ref _success);
    public long Failure => Volatile.Read(ref _failure);
    public IReadOnlyCollection<long> Latencies => _latencies;
    public IReadOnlyDictionary<int, long> StatusCodes => _statusCodes;
    public IReadOnlyDictionary<string, long> ExceptionTypes => _exceptionTypes;

    public long ReserveRequest()
    {
        return Interlocked.Increment(ref _attempted);
    }

    public bool TryReserveRequest(long requestLimit, out long requestNumber)
    {
        while (true)
        {
            var current = Volatile.Read(ref _attempted);

            if (current >= requestLimit)
            {
                requestNumber = 0;
                return false;
            }

            var next = current + 1;

            if (Interlocked.CompareExchange(ref _attempted, next, current) == current)
            {
                requestNumber = next;
                return true;
            }
        }
    }

    public void RecordCompletion(bool success)
    {
        Interlocked.Increment(ref _completed);

        if (success)
            Interlocked.Increment(ref _success);
        else
            Interlocked.Increment(ref _failure);
    }

    public void Merge(WorkerResult workerResult)
    {
        _latencies.AddRange(workerResult.Latencies);

        foreach (var (statusCode, count) in workerResult.StatusCodes)
            _statusCodes[statusCode] = _statusCodes.GetValueOrDefault(statusCode) + count;

        foreach (var (exceptionType, count) in workerResult.ExceptionTypes)
            _exceptionTypes[exceptionType] = _exceptionTypes.GetValueOrDefault(exceptionType) + count;
    }
}
