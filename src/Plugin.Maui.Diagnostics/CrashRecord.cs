namespace Plugin.Maui.Diagnostics;

/// <summary>
/// A crash recovered from the previous process, including the timeline that led to it.
/// </summary>
public sealed class CrashRecord
{
    /// <summary>Creates a crash record.</summary>
    public CrashRecord(
        DateTimeOffset occurredAt,
        string sessionId,
        ExceptionRecord exception,
        IReadOnlyList<TimelineEntry> timelineBeforeCrash)
    {
        OccurredAt = occurredAt;
        SessionId = sessionId;
        Exception = exception;
        TimelineBeforeCrash = timelineBeforeCrash;
    }

    /// <summary>When the crash was written (UTC).</summary>
    public DateTimeOffset OccurredAt { get; }

    /// <summary>Session that crashed.</summary>
    public string SessionId { get; }

    /// <summary>The fatal exception.</summary>
    public ExceptionRecord Exception { get; }

    /// <summary>Breadcrumbs persisted immediately before the crash.</summary>
    public IReadOnlyList<TimelineEntry> TimelineBeforeCrash { get; }

    /// <summary>
    /// Formats <see cref="TimelineBeforeCrash"/> as local <c>HH:mm:ss  Title</c> lines.
    /// </summary>
    public string FormatTimeline() => TimelineFormatter.Format(TimelineBeforeCrash);
}
