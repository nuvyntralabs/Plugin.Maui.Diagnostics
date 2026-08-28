namespace Plugin.Maui.Diagnostics;

interface IPlatformDiagnostics
{
    bool IsSupported { get; }

    void Enrich(DeviceSnapshotBuilder builder);

    IDisposable? Watch(IPlatformDiagnosticsListener listener);
}

interface IPlatformDiagnosticsListener
{
    void OnNativeCrash(string type, string message, string? stackTrace);

    void OnMemoryWarning();
}
