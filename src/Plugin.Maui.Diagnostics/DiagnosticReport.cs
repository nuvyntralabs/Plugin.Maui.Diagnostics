namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Full diagnostic dump: environment, session context, timeline, crash, and ANR.
/// </summary>
public sealed class DiagnosticReport
{
    /// <summary>Creates a diagnostic report.</summary>
    public DiagnosticReport(
        DateTimeOffset generatedAt,
        DeviceSnapshot device,
        SessionSnapshot session,
        IReadOnlyList<TimelineEntry> timeline,
        CrashRecord? lastCrash,
        AnrRecord? lastAnr,
        IReadOnlyList<ExceptionRecord> recentExceptions,
        IReadOnlyList<ApiCallRecord> recentApiCalls)
    {
        GeneratedAt = generatedAt;
        Device = device;
        Session = session;
        Timeline = timeline;
        LastCrash = lastCrash;
        LastAnr = lastAnr;
        RecentExceptions = recentExceptions;
        RecentApiCalls = recentApiCalls;
    }

    /// <summary>When the report was generated (UTC).</summary>
    public DateTimeOffset GeneratedAt { get; }

    /// <summary>Device, OS, app, memory, storage, battery, and connectivity.</summary>
    public DeviceSnapshot Device { get; }

    /// <summary>Last screen, last action, last successful API, uptime.</summary>
    public SessionSnapshot Session { get; }

    /// <summary>Current-session breadcrumbs, oldest first.</summary>
    public IReadOnlyList<TimelineEntry> Timeline { get; }

    /// <summary>Crash recovered from the previous process, if any.</summary>
    public CrashRecord? LastCrash { get; }

    /// <summary>Most recent ANR in this session, if any.</summary>
    public AnrRecord? LastAnr { get; }

    /// <summary>Recent tracked or unhandled exceptions.</summary>
    public IReadOnlyList<ExceptionRecord> RecentExceptions { get; }

    /// <summary>Recent API attempts.</summary>
    public IReadOnlyList<ApiCallRecord> RecentApiCalls { get; }

    /// <summary>
    /// Formats the current-session timeline as local <c>HH:mm:ss  Title</c> lines.
    /// </summary>
    public string FormatTimeline() => TimelineFormatter.Format(Timeline);

    /// <summary>
    /// Prefers the crash timeline when a previous crash exists; otherwise the live timeline.
    /// </summary>
    public string FormatWhatHappened() =>
        LastCrash is { TimelineBeforeCrash.Count: > 0 } crash
            ? crash.FormatTimeline()
            : FormatTimeline();
}
