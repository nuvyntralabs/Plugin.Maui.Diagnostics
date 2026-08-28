namespace Plugin.Maui.Diagnostics;

/// <summary>
/// One breadcrumb on the "what happened before the crash" timeline.
/// </summary>
public sealed class TimelineEntry
{
    /// <summary>
    /// Creates a timeline entry.
    /// </summary>
    public TimelineEntry(
        DateTimeOffset timestamp,
        DiagnosticEventKind kind,
        string title,
        string? detail = null,
        IReadOnlyDictionary<string, string>? properties = null)
    {
        Timestamp = timestamp;
        Kind = kind;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Detail = detail;
        Properties = properties ?? new Dictionary<string, string>();
    }

    /// <summary>UTC timestamp.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Event kind.</summary>
    public DiagnosticEventKind Kind { get; }

    /// <summary>Short label shown on the timeline (for example <c>Login Success</c>).</summary>
    public string Title { get; }

    /// <summary>Optional extra context (URL, exception type, duration).</summary>
    public string? Detail { get; }

    /// <summary>Structured properties attached to the breadcrumb.</summary>
    public IReadOnlyDictionary<string, string> Properties { get; }

    /// <summary>
    /// Formats the entry as <c>HH:mm:ss  Title</c> in local time.
    /// </summary>
    public string Format() => Format(TimeZoneInfo.Local);

    /// <summary>
    /// Formats the entry as <c>HH:mm:ss  Title</c> in the given time zone.
    /// </summary>
    public string Format(TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        var local = TimeZoneInfo.ConvertTime(Timestamp, timeZone);
        return $"{local:HH:mm:ss}  {Title}";
    }

    /// <inheritdoc />
    public override string ToString() => Format();
}
