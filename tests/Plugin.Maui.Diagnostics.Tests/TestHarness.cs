namespace Plugin.Maui.Diagnostics.Tests;

sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 8, 28, 10, 21, 3, TimeSpan.Zero);

    public void Advance(TimeSpan duration) => UtcNow += duration;

    public void Set(int hour, int minute, int second) =>
        UtcNow = new DateTimeOffset(2026, 8, 28, hour, minute, second, TimeSpan.Zero);
}

sealed class FakeEnvironment : IEnvironmentCollector
{
    public DeviceSnapshot Snapshot { get; set; } = new()
    {
        Manufacturer = "Acme",
        Model = "Pixel Test",
        Platform = "Android",
        OsVersion = "16",
        AppVersion = "1.2.3",
        AppBuild = "45",
        BatteryPercent = 82,
        BatteryState = BatteryChargeState.Discharging,
        FreeStorageBytes = 8L * 1024 * 1024 * 1024,
        TotalStorageBytes = 64L * 1024 * 1024 * 1024,
        AvailableMemoryBytes = 2L * 1024 * 1024 * 1024,
        TotalMemoryBytes = 8L * 1024 * 1024 * 1024,
        HasNetwork = true,
        HasInternet = true,
        MemoryPressure = MemoryPressureKind.Normal
    };

    public DeviceSnapshot Collect() => Snapshot;
}

sealed class FakePlatform : IPlatformDiagnostics
{
    public bool IsSupported => true;

    public void Enrich(DeviceSnapshotBuilder builder)
    {
        builder.IsAirplaneMode ??= false;
    }

    public IDisposable? Watch(IPlatformDiagnosticsListener listener) => null;
}

static class Harness
{
    public static (MauiDiagnosticsImplementation Diagnostics, FakeClock Clock, FakeEnvironment Environment, string Root) Create(
        Action<MauiDiagnosticsOptions>? configure = null)
    {
        var root = Directory.CreateTempSubdirectory("maui-diagnostics-").FullName;
        var clock = new FakeClock();
        var environment = new FakeEnvironment();
        var options = TestOptions(root);
        configure?.Invoke(options);

        var diagnostics = MauiDiagnostics.Create(options, clock, environment, new FakePlatform());
        return (diagnostics, clock, environment, root);
    }

    public static MauiDiagnosticsOptions TestOptions(string root) => new()
    {
        StorageDirectory = root,
        PersistTimeline = true,
        EnableAnrWatchdog = false,
        CaptureUnhandledExceptions = false,
        CaptureUnobservedTaskExceptions = false,
        WatchConnectivity = false,
        AutoTrackNavigation = false,
        LowBatteryPercent = 0,
        LowStorageMegabytes = 0
    };
}
