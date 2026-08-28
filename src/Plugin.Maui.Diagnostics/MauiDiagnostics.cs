namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Entry point for the diagnostics plugin when dependency injection is not used.
/// </summary>
public static class MauiDiagnostics
{
    static IMauiDiagnostics? _current;

    /// <summary>
    /// Gets the shared <see cref="IMauiDiagnostics"/> instance.
    /// </summary>
    public static IMauiDiagnostics Current => _current ??= Create(new MauiDiagnosticsOptions());

    /// <summary>
    /// Records a custom event on the timeline (for example <c>CheckoutStarted</c>).
    /// </summary>
    public static void TrackEvent(string name, IReadOnlyDictionary<string, string>? properties = null) =>
        Current.TrackEvent(name, properties);

    /// <summary>
    /// Records an exception and its breadcrumb.
    /// </summary>
    public static void TrackException(Exception exception, IReadOnlyDictionary<string, string>? properties = null) =>
        Current.TrackException(exception, properties);

    /// <summary>
    /// Records the current screen.
    /// </summary>
    public static void TrackScreen(string name, IReadOnlyDictionary<string, string>? properties = null) =>
        Current.TrackScreen(name, properties);

    /// <summary>
    /// Records a user action.
    /// </summary>
    public static void TrackUserAction(string action, IReadOnlyDictionary<string, string>? properties = null) =>
        Current.TrackUserAction(action, properties);

    /// <summary>
    /// Records an API attempt.
    /// </summary>
    public static void TrackApiCall(ApiCallRecord call) => Current.TrackApiCall(call);

    /// <summary>
    /// Records a network-level failure.
    /// </summary>
    public static void TrackNetworkFailure(string? message = null, IReadOnlyDictionary<string, string>? properties = null) =>
        Current.TrackNetworkFailure(message, properties);

    /// <summary>
    /// Builds a full diagnostic report.
    /// </summary>
    public static Task<DiagnosticReport> GenerateReportAsync(CancellationToken cancellationToken = default) =>
        Current.GenerateReportAsync(cancellationToken);

    /// <summary>
    /// Formats the current-session timeline as local <c>HH:mm:ss  Title</c> lines.
    /// </summary>
    public static string FormatTimeline() => Current.FormatTimeline();

    /// <summary>
    /// Creates a diagnostics instance with MAUI environment probes and platform hooks.
    /// </summary>
    public static IMauiDiagnostics Create(MauiDiagnosticsOptions? options = null) =>
        new MauiDiagnosticsImplementation(
            options ?? new MauiDiagnosticsOptions(),
            SystemClock.Instance,
            new MauiEnvironmentCollector(),
            new PlatformDiagnostics());

    /// <summary>
    /// Replaces the shared instance. Intended for tests and custom implementations.
    /// </summary>
    public static void SetDefault(IMauiDiagnostics implementation) =>
        _current = implementation ?? throw new ArgumentNullException(nameof(implementation));

    internal static MauiDiagnosticsImplementation Create(
        MauiDiagnosticsOptions options,
        IClock clock,
        IEnvironmentCollector environment,
        IPlatformDiagnostics platform) =>
        new(options, clock, environment, platform);
}
