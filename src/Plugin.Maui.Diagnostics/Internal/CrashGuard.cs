namespace Plugin.Maui.Diagnostics;

sealed class CrashGuard : IDisposable
{
    readonly MauiDiagnosticsOptions _options;
    readonly Action<Exception, bool> _onException;
    bool _hooked;

    public CrashGuard(MauiDiagnosticsOptions options, Action<Exception, bool> onException)
    {
        _options = options;
        _onException = onException;
    }

    public void Start()
    {
        if (_hooked)
        {
            return;
        }

        if (_options.CaptureUnhandledExceptions)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandled;
        }

        if (_options.CaptureUnobservedTaskExceptions)
        {
            TaskScheduler.UnobservedTaskException += OnUnobserved;
        }

        _hooked = true;
    }

    public void Dispose()
    {
        if (!_hooked)
        {
            return;
        }

        AppDomain.CurrentDomain.UnhandledException -= OnUnhandled;
        TaskScheduler.UnobservedTaskException -= OnUnobserved;
        _hooked = false;
    }

    void OnUnhandled(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _onException(exception, true);
        }
    }

    void OnUnobserved(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _onException(e.Exception, true);
    }
}
