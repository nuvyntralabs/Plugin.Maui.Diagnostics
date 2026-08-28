namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Point-in-time device, OS, app, and environment measurements.
/// </summary>
public sealed class DeviceSnapshot
{
    /// <summary>Device manufacturer (for example Apple, Samsung).</summary>
    public string? Manufacturer { get; init; }

    /// <summary>Device model.</summary>
    public string? Model { get; init; }

    /// <summary>MAUI platform name (Android, iOS).</summary>
    public string? Platform { get; init; }

    /// <summary>OS version string.</summary>
    public string? OsVersion { get; init; }

    /// <summary>Phone, tablet, or desktop idiom.</summary>
    public string? Idiom { get; init; }

    /// <summary>True when running in a simulator or emulator.</summary>
    public bool IsVirtualDevice { get; init; }

    /// <summary>Marketing app version.</summary>
    public string? AppVersion { get; init; }

    /// <summary>Build / CFBundleVersion / versionCode.</summary>
    public string? AppBuild { get; init; }

    /// <summary>Application package / bundle id.</summary>
    public string? PackageName { get; init; }

    /// <summary>Battery charge 0–100, or null when unknown.</summary>
    public double? BatteryPercent { get; init; }

    /// <summary>Charging state.</summary>
    public BatteryChargeState BatteryState { get; init; }

    /// <summary>True when Low Power Mode / Battery Saver is on.</summary>
    public bool? EnergySaverOn { get; init; }

    /// <summary>Free bytes on the app data volume.</summary>
    public long? FreeStorageBytes { get; init; }

    /// <summary>Total bytes on the app data volume.</summary>
    public long? TotalStorageBytes { get; init; }

    /// <summary>Memory still available to the process or system.</summary>
    public long? AvailableMemoryBytes { get; init; }

    /// <summary>Total physical memory.</summary>
    public long? TotalMemoryBytes { get; init; }

    /// <summary>Managed heap size from <c>GC.GetTotalMemory</c>.</summary>
    public long? AppUsedMemoryBytes { get; init; }

    /// <summary>True when the OS flagged a low-memory condition.</summary>
    public bool? IsLowMemory { get; init; }

    /// <summary>Classified memory pressure.</summary>
    public MemoryPressureKind MemoryPressure { get; init; }

    /// <summary>True when a network interface is available.</summary>
    public bool? HasNetwork { get; init; }

    /// <summary>True when internet access is believed available.</summary>
    public bool? HasInternet { get; init; }

    /// <summary>True on cellular or other metered links.</summary>
    public bool? IsExpensive { get; init; }

    /// <summary>True when connectivity is constrained.</summary>
    public bool? IsConstrained { get; init; }

    /// <summary>True when airplane mode is on (Android only).</summary>
    public bool? IsAirplaneMode { get; init; }

    /// <summary>Active connection profiles (Wi-Fi, cellular, …).</summary>
    public IReadOnlyList<string> ConnectionProfiles { get; init; } = [];

    /// <summary>A debugger is attached to the process.</summary>
    public bool DebuggerAttached { get; init; }

    /// <summary>Used memory as a percent of total, when both values exist.</summary>
    public double? UsedMemoryPercent =>
        TotalMemoryBytes is > 0 && AvailableMemoryBytes is { } available
            ? Math.Clamp((TotalMemoryBytes.Value - available) * 100d / TotalMemoryBytes.Value, 0, 100)
            : null;

    /// <summary>Used storage as a percent of total, when both values exist.</summary>
    public double? UsedStoragePercent =>
        TotalStorageBytes is > 0 && FreeStorageBytes is { } free
            ? Math.Clamp((TotalStorageBytes.Value - free) * 100d / TotalStorageBytes.Value, 0, 100)
            : null;
}
