namespace Plugin.Maui.Diagnostics;

sealed class PersistedSession
{
    public string SessionId { get; set; } = "";

    public DateTimeOffset StartedAt { get; set; }

    public bool CleanShutdown { get; set; }

    public string? LastScreen { get; set; }

    public string? LastUserAction { get; set; }

    public PersistedApiCall? LastSuccessfulApi { get; set; }

    public List<PersistedEntry> Timeline { get; set; } = [];

    public List<PersistedException> Exceptions { get; set; } = [];

    public List<PersistedApiCall> ApiCalls { get; set; } = [];
}

sealed class PersistedCrash
{
    public DateTimeOffset OccurredAt { get; set; }

    public string SessionId { get; set; } = "";

    public PersistedException Exception { get; set; } = new();

    public List<PersistedEntry> Timeline { get; set; } = [];
}

sealed class PersistedEntry
{
    public DateTimeOffset Timestamp { get; set; }

    public DiagnosticEventKind Kind { get; set; }

    public string Title { get; set; } = "";

    public string? Detail { get; set; }

    public Dictionary<string, string> Properties { get; set; } = [];

    public TimelineEntry ToEntry() => new(Timestamp, Kind, Title, Detail, Properties);

    public static PersistedEntry From(TimelineEntry entry) => new()
    {
        Timestamp = entry.Timestamp,
        Kind = entry.Kind,
        Title = entry.Title,
        Detail = entry.Detail,
        Properties = entry.Properties.ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal)
    };
}

sealed class PersistedException
{
    public DateTimeOffset Timestamp { get; set; }

    public string Type { get; set; } = "";

    public string Message { get; set; } = "";

    public string? StackTrace { get; set; }

    public bool IsUnhandled { get; set; }

    public Dictionary<string, string> Properties { get; set; } = [];

    public ExceptionRecord ToRecord() =>
        new(Timestamp, Type, Message, StackTrace, IsUnhandled, Properties);

    public static PersistedException From(ExceptionRecord record) => new()
    {
        Timestamp = record.Timestamp,
        Type = record.Type,
        Message = record.Message,
        StackTrace = record.StackTrace,
        IsUnhandled = record.IsUnhandled,
        Properties = record.Properties.ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal)
    };
}

sealed class PersistedApiCall
{
    public DateTimeOffset Timestamp { get; set; }

    public string Method { get; set; } = "";

    public string Url { get; set; } = "";

    public ApiCallStatus Status { get; set; }

    public int? StatusCode { get; set; }

    public double? DurationMs { get; set; }

    public string? Error { get; set; }

    public int Attempt { get; set; } = 1;

    public ApiCallRecord ToRecord() => new(
        Timestamp,
        Method,
        Url,
        Status,
        StatusCode,
        DurationMs is { } ms ? TimeSpan.FromMilliseconds(ms) : null,
        Error,
        Attempt);

    public static PersistedApiCall From(ApiCallRecord record) => new()
    {
        Timestamp = record.Timestamp,
        Method = record.Method,
        Url = record.Url,
        Status = record.Status,
        StatusCode = record.StatusCode,
        DurationMs = record.Duration?.TotalMilliseconds,
        Error = record.Error,
        Attempt = record.Attempt
    };
}
