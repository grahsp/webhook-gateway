using System.Net;

namespace WebhookGateway.LoadGenerator;

internal sealed class WorkerResult
{
    private readonly List<long> _latencies = [];
    private readonly Dictionary<int, long> _statusCodes = [];
    private readonly Dictionary<string, long> _exceptionTypes = [];

    public IReadOnlyCollection<long> Latencies => _latencies;
    public IReadOnlyDictionary<int, long> StatusCodes => _statusCodes;
    public IReadOnlyDictionary<string, long> ExceptionTypes => _exceptionTypes;

    public bool Record(long latencyMs, HttpStatusCode? statusCode, string? exceptionType)
    {
        _latencies.Add(Math.Max(0, latencyMs));

        if (statusCode is not null)
        {
            var statusCodeNumber = (int)statusCode.Value;
            _statusCodes[statusCodeNumber] = _statusCodes.GetValueOrDefault(statusCodeNumber) + 1;

            return statusCodeNumber is >= 200 and <= 299;
        }

        if (exceptionType is not null)
        {
            _exceptionTypes[exceptionType] = _exceptionTypes.GetValueOrDefault(exceptionType) + 1;
        }

        return false;
    }
}
