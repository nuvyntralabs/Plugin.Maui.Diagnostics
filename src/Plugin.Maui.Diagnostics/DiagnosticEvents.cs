namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Raised when a breadcrumb is appended to the timeline.
/// </summary>
public sealed class TimelineUpdatedEventArgs : EventArgs
{
    /// <summary>Creates the event args.</summary>
    public TimelineUpdatedEventArgs(TimelineEntry entry) =>
        Entry = entry ?? throw new ArgumentNullException(nameof(entry));

    /// <summary>The breadcrumb that was just written.</summary>
    public TimelineEntry Entry { get; }
}

/// <summary>
/// Raised when the ANR watchdog detects a main-thread freeze.
/// </summary>
public sealed class AnrDetectedEventArgs : EventArgs
{
    /// <summary>Creates the event args.</summary>
    public AnrDetectedEventArgs(AnrRecord anr) =>
        Anr = anr ?? throw new ArgumentNullException(nameof(anr));

    /// <summary>The detected freeze.</summary>
    public AnrRecord Anr { get; }
}

/// <summary>
/// Raised when an unhandled exception is persisted as a crash.
/// </summary>
public sealed class CrashCapturedEventArgs : EventArgs
{
    /// <summary>Creates the event args.</summary>
    public CrashCapturedEventArgs(CrashRecord crash) =>
        Crash = crash ?? throw new ArgumentNullException(nameof(crash));

    /// <summary>The captured crash.</summary>
    public CrashRecord Crash { get; }
}

/// <summary>
/// Optional callbacks configured on <see cref="MauiDiagnosticsOptions"/>.
/// </summary>
public sealed class MauiDiagnosticsEvents
{
    /// <summary>Called after a timeline entry is stored.</summary>
    public Action<TimelineEntry>? OnTimelineUpdated { get; set; }

    /// <summary>Called after an ANR is recorded.</summary>
    public Action<AnrRecord>? OnAnrDetected { get; set; }

    /// <summary>Called after a crash is persisted.</summary>
    public Action<CrashRecord>? OnCrashCaptured { get; set; }
}
