namespace Plugin.Maui.Diagnostics;

/// <summary>
/// A detected application-not-responding / main-thread freeze.
/// </summary>
public sealed class AnrRecord
{
    /// <summary>Creates an ANR record.</summary>
    public AnrRecord(
        DateTimeOffset occurredAt,
        TimeSpan duration,
        string? lastScreen,
        string? lastUserAction,
        IReadOnlyList<TimelineEntry> timelineBeforeAnr)
    {
        OccurredAt = occurredAt;
        Duration = duration;
        LastScreen = lastScreen;
        LastUserAction = lastUserAction;
        TimelineBeforeAnr = timelineBeforeAnr;
    }

    /// <summary>When the freeze was detected (UTC).</summary>
    public DateTimeOffset OccurredAt { get; }

    /// <summary>How long the main thread failed to acknowledge a ping.</summary>
    public TimeSpan Duration { get; }

    /// <summary>Screen that was current when the freeze was detected.</summary>
    public string? LastScreen { get; }

    /// <summary>Last user action before the freeze.</summary>
    public string? LastUserAction { get; }

    /// <summary>Timeline snapshot at detection time.</summary>
    public IReadOnlyList<TimelineEntry> TimelineBeforeAnr { get; }
}
