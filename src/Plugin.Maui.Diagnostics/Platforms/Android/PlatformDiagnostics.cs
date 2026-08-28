#if ANDROID
#pragma warning disable CA1416, CA1422
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Java.Lang;
using MauiPlatform = Microsoft.Maui.ApplicationModel.Platform;
using Exception = System.Exception;
using JavaThread = Java.Lang.Thread;

namespace Plugin.Maui.Diagnostics;

sealed class PlatformDiagnostics : IPlatformDiagnostics
{
    public bool IsSupported => true;

    public void Enrich(DeviceSnapshotBuilder builder)
    {
        Try(() => CollectStorage(builder));
        Try(() => CollectMemory(builder));
        Try(() => CollectAirplane(builder));
    }

    public IDisposable? Watch(IPlatformDiagnosticsListener listener)
    {
        var previous = JavaThread.DefaultUncaughtExceptionHandler;
        JavaThread.DefaultUncaughtExceptionHandler = new ChainedUncaughtHandler(previous, listener);
        return new Unhook(previous);
    }

    static void CollectStorage(DeviceSnapshotBuilder builder)
    {
        var path = MauiPlatform.AppContext.FilesDir?.AbsolutePath;
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var stat = new StatFs(path);
        builder.FreeStorageBytes = stat.AvailableBytes;
        builder.TotalStorageBytes = stat.TotalBytes;
    }

    static void CollectMemory(DeviceSnapshotBuilder builder)
    {
        if (MauiPlatform.AppContext.GetSystemService(Context.ActivityService) is not ActivityManager manager)
        {
            return;
        }

        var info = new ActivityManager.MemoryInfo();
        manager.GetMemoryInfo(info);
        builder.AvailableMemoryBytes = info.AvailMem;
        builder.TotalMemoryBytes = info.TotalMem;
        builder.IsLowMemory = info.LowMemory;

        if (info.LowMemory)
        {
            builder.MemoryPressure = MemoryPressureKind.Critical;
        }
        else if (info.TotalMem > 0 && info.AvailMem <= info.Threshold)
        {
            builder.MemoryPressure = MemoryPressureKind.Warning;
        }
        else
        {
            builder.MemoryPressure = MemoryPressureKind.Normal;
        }
    }

    static void CollectAirplane(DeviceSnapshotBuilder builder)
    {
        builder.IsAirplaneMode = Settings.Global.GetInt(
            MauiPlatform.AppContext.ContentResolver,
            Settings.Global.AirplaneModeOn,
            0) == 1;
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

    sealed class ChainedUncaughtHandler(JavaThread.IUncaughtExceptionHandler? previous, IPlatformDiagnosticsListener listener)
        : Java.Lang.Object, JavaThread.IUncaughtExceptionHandler
    {
        public void UncaughtException(JavaThread thread, Throwable exception)
        {
            try
            {
                listener.OnNativeCrash(
                    exception.Class?.Name ?? "Java.Lang.Throwable",
                    exception.Message ?? "Uncaught Java exception",
                    exception.GetStackTrace() is { Length: > 0 } frames
                        ? string.Join(System.Environment.NewLine, frames.Select(static frame => frame.ToString()))
                        : exception.ToString());
            }
            catch
            {
                // Never interfere with the original crash path.
            }

            previous?.UncaughtException(thread, exception);
        }
    }

    sealed class Unhook(JavaThread.IUncaughtExceptionHandler? previous) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                JavaThread.DefaultUncaughtExceptionHandler = previous;
            }
            catch
            {
                // Process may already be tearing down.
            }
        }
    }
}
#endif
