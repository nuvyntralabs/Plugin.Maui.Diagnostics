namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Configuration for a <see cref="IMauiDiagnostics"/> instance.
/// </summary>
public sealed class MauiDiagnosticsOptions
{
    /// <summary>
    /// Capture <see cref="AppDomain.UnhandledException"/> as a crash.
    /// </summary>
    public bool CaptureUnhandledExceptions { get; set; } = true;

    /// <summary>
    /// Capture <see cref="TaskScheduler.UnobservedTaskException"/>.
    /// </summary>
    public bool CaptureUnobservedTaskExceptions { get; set; } = true;

    /// <summary>
    /// Ping the main thread and record an ANR when it stops responding.
    /// </summary>
    public bool EnableAnrWatchdog { get; set; } = true;

    /// <summary>
    /// How long the main thread may stay frozen before an ANR is recorded.
    /// </summary>
    public TimeSpan AnrThreshold { get; set; } = MauiDiagnosticsDefaults.AnrThreshold;

    /// <summary>
    /// How often the watchdog posts a ping to the main thread.
    /// </summary>
    public TimeSpan AnrPollInterval { get; set; } = MauiDiagnosticsDefaults.AnrPollInterval;

    /// <summary>
    /// Minimum time between ANR reports.
    /// </summary>
    public TimeSpan AnrCooldown { get; set; } = MauiDiagnosticsDefaults.AnrCooldown;

    /// <summary>
    /// Maximum breadcrumbs kept in memory and on disk.
    /// </summary>
    public int MaxTimelineEntries { get; set; } = MauiDiagnosticsDefaults.MaxTimelineEntries;

    /// <summary>
    /// Maximum recent exceptions kept on a report.
    /// </summary>
    public int MaxRecentExceptions { get; set; } = MauiDiagnosticsDefaults.MaxRecentExceptions;

    /// <summary>
    /// Maximum recent API calls kept on a report.
    /// </summary>
    public int MaxRecentApiCalls { get; set; } = MauiDiagnosticsDefaults.MaxRecentApiCalls;

    /// <summary>
    /// Persist the timeline so the next launch can show what happened before a crash.
    /// </summary>
    public bool PersistTimeline { get; set; } = true;

    /// <summary>
    /// Override the persistence folder. Tests and custom hosts set this.
    /// When null, files go under app data / <see cref="MauiDiagnosticsDefaults.StorageFolderName"/>.
    /// </summary>
    public string? StorageDirectory { get; set; }

    /// <summary>
    /// Subscribe to MAUI connectivity changes and write Network Lost / Restored.
    /// </summary>
    public bool WatchConnectivity { get; set; } = true;

    /// <summary>
    /// Subscribe to page appearing events and record screens automatically.
    /// </summary>
    public bool AutoTrackNavigation { get; set; } = true;

    /// <summary>
    /// Include exception stack traces in records and reports.
    /// </summary>
    public bool IncludeStackTraces { get; set; } = true;

    /// <summary>
    /// Strip query strings from tracked URLs.
    /// </summary>
    public bool RedactUrlQuery { get; set; } = true;

    /// <summary>
    /// Write a timeline warning when battery falls to this percent or below.
    /// Set to 0 to disable.
    /// </summary>
    public double LowBatteryPercent { get; set; } = MauiDiagnosticsDefaults.LowBatteryPercent;

    /// <summary>
    /// Write a timeline warning when free storage falls to this many megabytes or below.
    /// Set to 0 to disable.
    /// </summary>
    public long LowStorageMegabytes { get; set; } = MauiDiagnosticsDefaults.LowStorageMegabytes;

    /// <summary>
    /// HTTP status codes treated as API success. Defaults to 2xx.
    /// </summary>
    public Func<int, bool>? IsSuccessStatusCode { get; set; }

    /// <summary>
    /// Optional diagnostic callbacks.
    /// </summary>
    public MauiDiagnosticsEvents Events { get; set; } = new();
}
