namespace Plugin.Maui.Diagnostics;

/// <summary>
/// A tracked or unhandled exception.
/// </summary>
public sealed class ExceptionRecord
{
    /// <summary>Creates an exception record.</summary>
    public ExceptionRecord(
        DateTimeOffset timestamp,
        string type,
        string message,
        string? stackTrace,
        bool isUnhandled,
        IReadOnlyDictionary<string, string>? properties = null)
    {
        Timestamp = timestamp;
        Type = type;
        Message = message;
        StackTrace = stackTrace;
        IsUnhandled = isUnhandled;
        Properties = properties ?? new Dictionary<string, string>();
    }

    /// <summary>When the exception was captured (UTC).</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>CLR exception type name.</summary>
    public string Type { get; }

    /// <summary>Exception message.</summary>
    public string Message { get; }

    /// <summary>Stack trace when capture is enabled.</summary>
    public string? StackTrace { get; }

    /// <summary>True for unhandled / crash-path exceptions.</summary>
    public bool IsUnhandled { get; }

    /// <summary>Caller-supplied properties.</summary>
    public IReadOnlyDictionary<string, string> Properties { get; }
}
