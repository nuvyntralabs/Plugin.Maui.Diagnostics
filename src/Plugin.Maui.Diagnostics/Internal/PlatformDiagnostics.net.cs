#if !ANDROID && !IOS
namespace Plugin.Maui.Diagnostics;

sealed class PlatformDiagnostics : IPlatformDiagnostics
{
    public bool IsSupported => false;

    public void Enrich(DeviceSnapshotBuilder builder)
    {
    }

    public IDisposable? Watch(IPlatformDiagnosticsListener listener) => null;
}
#endif
