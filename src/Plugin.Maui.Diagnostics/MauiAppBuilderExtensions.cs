using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Plugin.Maui.Diagnostics;

/// <summary>
/// MAUI host registration for diagnostics.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="IMauiDiagnostics"/> as a singleton and wires Android/iOS
    /// lifecycle hooks for foreground, background, and memory warnings.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.UseMauiDiagnostics(options =>
    /// {
    ///     options.EnableAnrWatchdog = true;
    ///     options.AnrThreshold = TimeSpan.FromSeconds(5);
    /// });
    /// </code>
    /// </example>
    public static MauiAppBuilder UseMauiDiagnostics(this MauiAppBuilder builder, Action<MauiDiagnosticsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new MauiDiagnosticsOptions();
        configure?.Invoke(options);

        builder.Services.AddMauiDiagnostics(options);
        builder.Services.AddTransient<IMauiInitializeService, MauiDiagnosticsInitializer>();

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnResume(_ => MauiDiagnostics.Current.NotifyForeground());
                android.OnPause(_ => MauiDiagnostics.Current.NotifyBackground());
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.OnActivated(_ => MauiDiagnostics.Current.NotifyForeground());
                ios.DidEnterBackground(_ => MauiDiagnostics.Current.NotifyBackground());
            });
#endif
        });

        return builder;
    }
}
