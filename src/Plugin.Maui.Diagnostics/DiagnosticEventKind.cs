namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Kind of breadcrumb written to the diagnostics timeline.
/// </summary>
public enum DiagnosticEventKind
{
    /// <summary>The process started a new diagnostics session.</summary>
    AppStarted,

    /// <summary>The app returned to the foreground.</summary>
    AppForeground,

    /// <summary>The app moved to the background.</summary>
    AppBackground,

    /// <summary>A custom application event.</summary>
    Event,

    /// <summary>A screen or page became current.</summary>
    Screen,

    /// <summary>A user interaction was recorded.</summary>
    UserAction,

    /// <summary>An HTTP / API request was sent.</summary>
    ApiRequest,

    /// <summary>An HTTP / API request completed successfully.</summary>
    ApiSuccess,

    /// <summary>An HTTP / API request failed.</summary>
    ApiFailure,

    /// <summary>An HTTP / API request is being retried.</summary>
    ApiRetry,

    /// <summary>Connectivity dropped.</summary>
    NetworkLost,

    /// <summary>Connectivity returned.</summary>
    NetworkRestored,

    /// <summary>A network-level failure (timeout, DNS, reset).</summary>
    NetworkFailure,

    /// <summary>A handled or tracked exception.</summary>
    Exception,

    /// <summary>An unhandled exception or native crash was captured.</summary>
    Crash,

    /// <summary>The main thread was frozen long enough to count as ANR.</summary>
    Anr,

    /// <summary>The OS reported memory pressure.</summary>
    MemoryPressure,

    /// <summary>Battery crossed a notable threshold.</summary>
    Battery,

    /// <summary>Storage crossed a notable threshold.</summary>
    Storage
}
