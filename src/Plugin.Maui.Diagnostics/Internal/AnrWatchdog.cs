namespace Plugin.Maui.Diagnostics;

sealed class AnrWatchdog : IDisposable
{
    readonly MauiDiagnosticsOptions _options;
    readonly IClock _clock;
    readonly Action<TimeSpan> _onAnr;
    readonly object _gate = new();

    SynchronizationContext? _main;
    Timer? _timer;
    int _pendingPing;
    DateTimeOffset _pingSentAt;
    DateTimeOffset _lastAnr = DateTimeOffset.MinValue;
    bool _disposed;

    public AnrWatchdog(MauiDiagnosticsOptions options, IClock clock, Action<TimeSpan> onAnr)
    {
        _options = options;
        _clock = clock;
        _onAnr = onAnr;
    }

    public void Start(SynchronizationContext? mainContext)
    {
        if (!_options.EnableAnrWatchdog || _timer is not null)
        {
            return;
        }

        _main = mainContext ?? SynchronizationContext.Current;
        if (_main is null)
        {
            return;
        }

        _timer = new Timer(OnTick, null, _options.AnrPollInterval, _options.AnrPollInterval);
    }

    public void Pulse() => OnTick(null);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _timer?.Dispose();
        _timer = null;
    }

    void OnTick(object? _)
    {
        if (_disposed || _main is null)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _pendingPing, 1, 0) == 1)
        {
            var waited = _clock.UtcNow - _pingSentAt;
            if (waited >= _options.AnrThreshold)
            {
                var now = _clock.UtcNow;
                lock (_gate)
                {
                    if (now - _lastAnr < _options.AnrCooldown)
                    {
                        return;
                    }

                    _lastAnr = now;
                }

                Interlocked.Exchange(ref _pendingPing, 0);
                _onAnr(waited);
            }

            return;
        }

        _pingSentAt = _clock.UtcNow;
        try
        {
            _main.Post(_ => Interlocked.Exchange(ref _pendingPing, 0), null);
        }
        catch
        {
            Interlocked.Exchange(ref _pendingPing, 0);
        }
    }
}
