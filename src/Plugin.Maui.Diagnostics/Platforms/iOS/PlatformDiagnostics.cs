#if IOS
using System.Runtime.InteropServices;
using Foundation;
using UIKit;

namespace Plugin.Maui.Diagnostics;

sealed class PlatformDiagnostics : IPlatformDiagnostics
{
    public bool IsSupported => true;

    public void Enrich(DeviceSnapshotBuilder builder)
    {
        Try(() => CollectStorage(builder));
        Try(() => CollectMemory(builder));
    }

    public IDisposable? Watch(IPlatformDiagnosticsListener listener)
    {
        var memory = NSNotificationCenter.DefaultCenter.AddObserver(
            UIApplication.DidReceiveMemoryWarningNotification,
            _ => listener.OnMemoryWarning());

        return new Unhook(memory);
    }

    static void CollectStorage(DeviceSnapshotBuilder builder)
    {
        var path = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User).FirstOrDefault();
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var attributes = NSFileManager.DefaultManager.GetFileSystemAttributes(path, out _);
        if (attributes is null)
        {
            return;
        }

        builder.FreeStorageBytes = (long)attributes.FreeSize;
        builder.TotalStorageBytes = (long)attributes.Size;
    }

    static void CollectMemory(DeviceSnapshotBuilder builder)
    {
        builder.TotalMemoryBytes = (long)NSProcessInfo.ProcessInfo.PhysicalMemory;

        var available = (long)OsProcAvailableMemory();
        if (available >= 0)
        {
            builder.AvailableMemoryBytes = available;
        }

        if (builder.TotalMemoryBytes is not > 0 || builder.AvailableMemoryBytes is not { } remaining)
        {
            return;
        }

        var usedPercent = (builder.TotalMemoryBytes.Value - remaining) * 100d / builder.TotalMemoryBytes.Value;
        builder.MemoryPressure = usedPercent >= 92
            ? MemoryPressureKind.Critical
            : usedPercent >= 80
                ? MemoryPressureKind.Warning
                : MemoryPressureKind.Normal;
    }

    static void Try(Action collect)
    {
        try
        {
            collect();
        }
        catch
        {
            // Platform probes must never throw into the host app.
        }
    }

    [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "os_proc_available_memory")]
    static extern nuint OsProcAvailableMemory();

    sealed class Unhook(NSObject token) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(token);
            }
            catch
            {
                // Process may already be tearing down.
            }
        }
    }
}
#endif
