#if ANDROID || IOS
using Microsoft.Maui.Storage;
#endif

namespace Plugin.Maui.Diagnostics;

static class StoragePath
{
    public static string Resolve(MauiDiagnosticsOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.StorageDirectory))
        {
            return options.StorageDirectory;
        }

        var root = TryAppData() ?? Path.Combine(Path.GetTempPath(), MauiDiagnosticsDefaults.StorageFolderName);
        return Path.Combine(root, MauiDiagnosticsDefaults.StorageFolderName);
    }

    static string? TryAppData()
    {
#if ANDROID || IOS
        try
        {
            return FileSystem.AppDataDirectory;
        }
        catch
        {
            return null;
        }
#else
        return null;
#endif
    }
}
