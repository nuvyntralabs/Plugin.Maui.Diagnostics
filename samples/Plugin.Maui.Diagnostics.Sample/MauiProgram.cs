using Microsoft.Extensions.Logging;
using Plugin.Maui.Diagnostics;

namespace Plugin.Maui.Diagnostics.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<OrderPage>();

        builder
            .UseMauiApp<App>()
            .UseMauiDiagnostics(options =>
            {
                options.EnableAnrWatchdog = true;
                options.AnrThreshold = TimeSpan.FromSeconds(4);
                options.PersistTimeline = true;
                options.AutoTrackNavigation = true;
                options.WatchConnectivity = true;
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
