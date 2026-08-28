namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Registers diagnostics services without MAUI lifecycle hooks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IMauiDiagnostics"/> using the supplied options instance.
    /// </summary>
    public static IServiceCollection AddMauiDiagnostics(this IServiceCollection services, MauiDiagnosticsOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.TryAddSingleton<IMauiDiagnostics>(sp =>
        {
            var diagnostics = MauiDiagnostics.Create(options);
            MauiDiagnostics.SetDefault(diagnostics);
            return diagnostics;
        });

        return services;
    }

    /// <summary>
    /// Adds <see cref="IMauiDiagnostics"/> and applies <paramref name="configure"/> to a new options instance.
    /// </summary>
    public static IServiceCollection AddMauiDiagnostics(this IServiceCollection services, Action<MauiDiagnosticsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new MauiDiagnosticsOptions();
        configure?.Invoke(options);
        return services.AddMauiDiagnostics(options);
    }
}
