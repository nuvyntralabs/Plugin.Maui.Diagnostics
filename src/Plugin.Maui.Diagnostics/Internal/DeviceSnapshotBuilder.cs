namespace Plugin.Maui.Diagnostics;

sealed class DeviceSnapshotBuilder
{
    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? Platform { get; set; }

    public string? OsVersion { get; set; }

    public string? Idiom { get; set; }

    public bool IsVirtualDevice { get; set; }

    public string? AppVersion { get; set; }

    public string? AppBuild { get; set; }

    public string? PackageName { get; set; }

    public double? BatteryPercent { get; set; }

    public BatteryChargeState BatteryState { get; set; }

    public bool? EnergySaverOn { get; set; }

    public long? FreeStorageBytes { get; set; }

    public long? TotalStorageBytes { get; set; }

    public long? AvailableMemoryBytes { get; set; }

    public long? TotalMemoryBytes { get; set; }

    public long? AppUsedMemoryBytes { get; set; }

    public bool? IsLowMemory { get; set; }

    public MemoryPressureKind MemoryPressure { get; set; }

    public bool? HasNetwork { get; set; }

    public bool? HasInternet { get; set; }

    public bool? IsExpensive { get; set; }

    public bool? IsConstrained { get; set; }

    public bool? IsAirplaneMode { get; set; }

    public List<string> ConnectionProfiles { get; } = [];

    public bool DebuggerAttached { get; set; }

    public DeviceSnapshot Build() => new()
    {
        Manufacturer = Manufacturer,
        Model = Model,
        Platform = Platform,
        OsVersion = OsVersion,
        Idiom = Idiom,
        IsVirtualDevice = IsVirtualDevice,
        AppVersion = AppVersion,
        AppBuild = AppBuild,
        PackageName = PackageName,
        BatteryPercent = BatteryPercent,
        BatteryState = BatteryState,
        EnergySaverOn = EnergySaverOn,
        FreeStorageBytes = FreeStorageBytes,
        TotalStorageBytes = TotalStorageBytes,
        AvailableMemoryBytes = AvailableMemoryBytes,
        TotalMemoryBytes = TotalMemoryBytes,
        AppUsedMemoryBytes = AppUsedMemoryBytes,
        IsLowMemory = IsLowMemory,
        MemoryPressure = MemoryPressure,
        HasNetwork = HasNetwork,
        HasInternet = HasInternet,
        IsExpensive = IsExpensive,
        IsConstrained = IsConstrained,
        IsAirplaneMode = IsAirplaneMode,
        ConnectionProfiles = ConnectionProfiles.ToArray(),
        DebuggerAttached = DebuggerAttached
    };
}
