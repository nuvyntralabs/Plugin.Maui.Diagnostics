namespace Plugin.Maui.Diagnostics;

sealed class FallbackEnvironmentCollector : IEnvironmentCollector
{
    readonly DeviceSnapshot _snapshot;

    public FallbackEnvironmentCollector(DeviceSnapshot? snapshot = null) =>
        _snapshot = snapshot ?? new DeviceSnapshot
        {
            Platform = "net",
            OsVersion = Environment.OSVersion.VersionString,
            AppVersion = "test",
            AppBuild = "0",
            DebuggerAttached = Debugger.IsAttached,
            AppUsedMemoryBytes = GC.GetTotalMemory(false)
        };

    public DeviceSnapshot Collect() => _snapshot;
}
