namespace Plugin.Maui.Diagnostics;

/// <summary>
/// A recorded HTTP / API attempt.
/// </summary>
public sealed class ApiCallRecord
{
    /// <summary>Creates an API call record.</summary>
    public ApiCallRecord(
        DateTimeOffset timestamp,
        string method,
        string url,
        ApiCallStatus status,
        int? statusCode = null,
        TimeSpan? duration = null,
        string? error = null,
        int attempt = 1)
    {
        Timestamp = timestamp;
        Method = method;
        Url = url;
        Status = status;
        StatusCode = statusCode;
        Duration = duration;
        Error = error;
        Attempt = attempt;
    }

    /// <summary>When the attempt was recorded (UTC).</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>HTTP method.</summary>
    public string Method { get; }

    /// <summary>Request URL, query-stripped when redaction is on.</summary>
    public string Url { get; }

    /// <summary>Outcome.</summary>
    public ApiCallStatus Status { get; }

    /// <summary>HTTP status code when a response arrived.</summary>
    public int? StatusCode { get; }

    /// <summary>Elapsed time for the attempt.</summary>
    public TimeSpan? Duration { get; }

    /// <summary>Failure message.</summary>
    public string? Error { get; }

    /// <summary>1-based attempt number (retries increment this).</summary>
    public int Attempt { get; }

    /// <summary>True when the call completed with a success status.</summary>
    public bool IsSuccess => Status == ApiCallStatus.Succeeded;
}
