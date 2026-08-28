namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Default values for <see cref="MauiDiagnosticsOptions"/>.
/// </summary>
public static class MauiDiagnosticsDefaults
{
    /// <summary>How many timeline breadcrumbs to keep in memory and on disk.</summary>
    public const int MaxTimelineEntries = 200;

    /// <summary>How many recent exceptions to keep on the report.</summary>
    public const int MaxRecentExceptions = 20;

    /// <summary>How many recent API calls to keep on the report.</summary>
    public const int MaxRecentApiCalls = 20;

    /// <summary>Main-thread freeze duration treated as ANR.</summary>
    public static readonly TimeSpan AnrThreshold = TimeSpan.FromSeconds(5);

    /// <summary>How often the ANR watchdog pings the main thread.</summary>
    public static readonly TimeSpan AnrPollInterval = TimeSpan.FromSeconds(1);

    /// <summary>Minimum gap between repeated ANR reports.</summary>
    public static readonly TimeSpan AnrCooldown = TimeSpan.FromSeconds(15);

    /// <summary>Battery percent that writes a timeline warning.</summary>
    public const double LowBatteryPercent = 15;

    /// <summary>Free storage (MB) that writes a timeline warning.</summary>
    public const long LowStorageMegabytes = 256;

    /// <summary>Folder name under app data used for persisted diagnostics.</summary>
    public const string StorageFolderName = "maui-diagnostics";
}
