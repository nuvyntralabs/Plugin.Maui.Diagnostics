using Microsoft.Maui.Hosting;

namespace Plugin.Maui.Diagnostics;

sealed class MauiDiagnosticsInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var diagnostics = services.GetService<IMauiDiagnostics>() ?? MauiDiagnostics.Current;
        MauiDiagnostics.SetDefault(diagnostics);
        diagnostics.Start();
    }
}
