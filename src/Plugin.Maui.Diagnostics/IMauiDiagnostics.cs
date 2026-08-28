namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Production diagnostics: breadcrumbs, crash / ANR capture, and a structured report.
/// </summary>
public interface IMauiDiagnostics
{
    /// <summary>
    /// Gets a value indicating whether this target can collect diagnostics.
    /// Always <c>true</c> for Android, iOS, and the shared <c>net10.0</c> surface.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets a value indicating whether the session has been started.
    /// </summary>
    bool IsStarted { get; }

    /// <summary>
    /// Crash recovered from the previous process, if any.
    /// </summary>
    CrashRecord? LastCrash { get; }

    /// <summary>
    /// Most recent ANR in this session, if any.
    /// </summary>
    AnrRecord? LastAnr { get; }

    /// <summary>
    /// Last tracked screen name.
    /// </summary>
    string? LastScreen { get; }

    /// <summary>
    /// Last tracked user action.
    /// </summary>
    string? LastUserAction { get; }

    /// <summary>
    /// Most recent successful API call.
    /// </summary>
    ApiCallRecord? LastSuccessfulApiCall { get; }

    /// <summary>
    /// Raised after a breadcrumb is appended.
    /// </summary>
    event EventHandler<TimelineUpdatedEventArgs>? TimelineUpdated;

    /// <summary>
    /// Raised after an ANR is recorded.
    /// </summary>
    event EventHandler<AnrDetectedEventArgs>? AnrDetected;

    /// <summary>
    /// Raised after a crash is persisted.
    /// </summary>
    event EventHandler<CrashCapturedEventArgs>? CrashCaptured;

    /// <summary>
    /// Starts a session, recovers any previous crash, and turns on watchers.
    /// Safe to call more than once.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops watchers and marks the session as a clean shutdown.
    /// </summary>
    void Stop();

    /// <summary>
    /// Records a custom event on the timeline (for example <c>CheckoutStarted</c>).
    /// </summary>
    void TrackEvent(string name, IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Records an exception and its breadcrumb.
    /// </summary>
    void TrackException(Exception exception, IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Records the current screen. Also used as "last screen" on the report.
    /// </summary>
    void TrackScreen(string name, IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Records a user action. Also used as "last user action" on the report.
    /// </summary>
    void TrackUserAction(string action, IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Records an API attempt (request, success, failure, or retry).
    /// </summary>
    void TrackApiCall(ApiCallRecord call);

    /// <summary>
    /// Records a network-level failure.
    /// </summary>
    void TrackNetworkFailure(string? message = null, IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Returns a snapshot of the current-session timeline, oldest first.
    /// </summary>
    IReadOnlyList<TimelineEntry> GetTimeline();

    /// <summary>
    /// Formats the current-session timeline as local <c>HH:mm:ss  Title</c> lines.
    /// </summary>
    string FormatTimeline();

    /// <summary>
    /// Builds a full diagnostic report: device, session, timeline, last crash, last ANR.
    /// </summary>
    Task<DiagnosticReport> GenerateReportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the app returns to the foreground.
    /// </summary>
    void NotifyForeground();

    /// <summary>
    /// Called when the app moves to the background.
    /// </summary>
    void NotifyBackground();

    /// <summary>
    /// Called when the OS reports a memory warning.
    /// </summary>
    void NotifyMemoryWarning();

    /// <summary>
    /// Clears in-memory breadcrumbs. Pass <paramref name="includePersistedCrash"/> to drop a recovered crash.
    /// </summary>
    void Clear(bool includePersistedCrash = false);
}
