namespace Plugin.Maui.Diagnostics;

sealed class MauiDiagnosticsImplementation : IMauiDiagnostics, IPlatformDiagnosticsListener, IDisposable
{
    readonly MauiDiagnosticsOptions _options;
    readonly IClock _clock;
    readonly IEnvironmentCollector _environment;
    readonly IPlatformDiagnostics _platform;
    readonly TimelineStore _timeline;
    readonly DiagnosticPersistence _persistence;
    readonly CrashGuard _crashGuard;
    readonly AnrWatchdog _anrWatchdog;
    readonly NetworkWatcher _networkWatcher;
    readonly NavigationWatcher _navigationWatcher;
    readonly object _gate = new();
    readonly List<ExceptionRecord> _exceptions = [];
    readonly List<ApiCallRecord> _apiCalls = [];

    string _sessionId = "";
    DateTimeOffset _startedAt;
    bool _started;
    bool _foreground = true;
    bool _disposed;
    string? _lastScreen;
    string? _lastUserAction;
    ApiCallRecord? _lastSuccessfulApi;
    AnrRecord? _lastAnr;
    CrashRecord? _lastCrash;
    IDisposable? _platformWatch;
    bool _lowBatteryNoted;
    bool _lowStorageNoted;
    bool _memoryNoted;

    public MauiDiagnosticsImplementation(
        MauiDiagnosticsOptions options,
        IClock clock,
        IEnvironmentCollector environment,
        IPlatformDiagnostics platform)
    {
        _options = options;
        _clock = clock;
        _environment = environment;
        _platform = platform;
        _timeline = new TimelineStore(options.MaxTimelineEntries);
        _persistence = new DiagnosticPersistence(StoragePath.Resolve(options), options.PersistTimeline);
        _crashGuard = new CrashGuard(options, OnCapturedException);
        _anrWatchdog = new AnrWatchdog(options, clock, OnAnr);
        _networkWatcher = new NetworkWatcher(OnNetworkChanged);
        _navigationWatcher = new NavigationWatcher(name => TrackScreen(name));
    }

    public bool IsSupported => true;

    public bool IsStarted => _started;

    public CrashRecord? LastCrash
    {
        get
        {
            lock (_gate)
            {
                return _lastCrash;
            }
        }
    }

    public AnrRecord? LastAnr
    {
        get
        {
            lock (_gate)
            {
                return _lastAnr;
            }
        }
    }

    public string? LastScreen
    {
        get
        {
            lock (_gate)
            {
                return _lastScreen;
            }
        }
    }

    public string? LastUserAction
    {
        get
        {
            lock (_gate)
            {
                return _lastUserAction;
            }
        }
    }

    public ApiCallRecord? LastSuccessfulApiCall
    {
        get
        {
            lock (_gate)
            {
                return _lastSuccessfulApi;
            }
        }
    }

    public event EventHandler<TimelineUpdatedEventArgs>? TimelineUpdated;

    public event EventHandler<AnrDetectedEventArgs>? AnrDetected;

    public event EventHandler<CrashCapturedEventArgs>? CrashCaptured;

    public void Start()
    {
        lock (_gate)
        {
            if (_started)
            {
                return;
            }

            RecoverLocked();
            _sessionId = Guid.NewGuid().ToString("N");
            _startedAt = _clock.UtcNow;
            _started = true;
            _foreground = true;
        }

        _crashGuard.Start();
        _platformWatch = _platform.Watch(this);
        if (_options.WatchConnectivity)
        {
            _networkWatcher.Start();
        }

        if (_options.AutoTrackNavigation)
        {
            _navigationWatcher.Start();
        }

        _anrWatchdog.Start(SynchronizationContext.Current);
        Append(DiagnosticEventKind.AppStarted, "App Started");
        NoteEnvironment(CollectDevice());
    }

    public void Stop()
    {
        lock (_gate)
        {
            if (!_started)
            {
                return;
            }

            PersistSessionLocked(cleanShutdown: true);
            _started = false;
        }

        DisposeWatchers();
    }

    public void TrackEvent(string name, IReadOnlyDictionary<string, string>? properties = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureStarted();
        Append(DiagnosticEventKind.Event, name, properties: properties);
    }

    public void TrackException(Exception exception, IReadOnlyDictionary<string, string>? properties = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        EnsureStarted();
        RecordException(exception, isUnhandled: false, persistCrash: false, properties);
    }

    public void TrackScreen(string name, IReadOnlyDictionary<string, string>? properties = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureStarted();
        lock (_gate)
        {
            _lastScreen = name;
        }

        Append(DiagnosticEventKind.Screen, name, properties: properties);
    }

    public void TrackUserAction(string action, IReadOnlyDictionary<string, string>? properties = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        EnsureStarted();
        lock (_gate)
        {
            _lastUserAction = action;
        }

        Append(DiagnosticEventKind.UserAction, action, properties: properties);
    }

    public void TrackApiCall(ApiCallRecord call)
    {
        ArgumentNullException.ThrowIfNull(call);
        EnsureStarted();

        var sanitized = new ApiCallRecord(
            call.Timestamp,
            call.Method,
            UrlSanitizer.Sanitize(call.Url, _options.RedactUrlQuery),
            call.Status,
            call.StatusCode,
            call.Duration,
            call.Error,
            call.Attempt);

        var kind = sanitized.Status switch
        {
            ApiCallStatus.Succeeded => DiagnosticEventKind.ApiSuccess,
            ApiCallStatus.Failed => IsNetworkError(sanitized.Error) ? DiagnosticEventKind.NetworkFailure : DiagnosticEventKind.ApiFailure,
            ApiCallStatus.Retried => DiagnosticEventKind.ApiRetry,
            _ => DiagnosticEventKind.ApiRequest
        };

        var title = kind switch
        {
            DiagnosticEventKind.ApiSuccess => "API Success",
            DiagnosticEventKind.ApiFailure => "API Failure",
            DiagnosticEventKind.ApiRetry => "API Retry",
            DiagnosticEventKind.NetworkFailure => "Network Failure",
            _ => "API Request"
        };

        var detail = $"{sanitized.Method} {sanitized.Url}";
        if (sanitized.StatusCode is { } code)
        {
            detail += $" ({code})";
        }

        lock (_gate)
        {
            Remember(_apiCalls, sanitized, _options.MaxRecentApiCalls);
            if (sanitized.IsSuccess)
            {
                _lastSuccessfulApi = sanitized;
            }
        }

        Append(kind, title, detail, ToProperties(sanitized));
    }

    public void TrackNetworkFailure(string? message = null, IReadOnlyDictionary<string, string>? properties = null)
    {
        EnsureStarted();
        Append(DiagnosticEventKind.NetworkFailure, "Network Failure", message, properties);
    }

    public IReadOnlyList<TimelineEntry> GetTimeline()
    {
        lock (_gate)
        {
            return _timeline.Snapshot();
        }
    }

    public string FormatTimeline() => TimelineFormatter.Format(GetTimeline());

    public Task<DiagnosticReport> GenerateReportAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureStarted();

        var device = CollectDevice();

        DiagnosticReport report;
        lock (_gate)
        {
            report = new DiagnosticReport(
                _clock.UtcNow,
                device,
                new SessionSnapshot(
                    _sessionId,
                    _startedAt,
                    _clock.UtcNow - _startedAt,
                    _lastScreen,
                    _lastUserAction,
                    _lastSuccessfulApi,
                    _foreground),
                _timeline.Snapshot(),
                _lastCrash,
                _lastAnr,
                _exceptions.ToArray(),
                _apiCalls.ToArray());
        }

        return Task.FromResult(report);
    }

    public void NotifyForeground()
    {
        EnsureStarted();
        lock (_gate)
        {
            _foreground = true;
        }

        Append(DiagnosticEventKind.AppForeground, "App Foreground");
    }

    public void NotifyBackground()
    {
        EnsureStarted();
        lock (_gate)
        {
            _foreground = false;
            PersistSessionLocked(cleanShutdown: false);
        }

        Append(DiagnosticEventKind.AppBackground, "App Background");
    }

    public void NotifyMemoryWarning()
    {
        EnsureStarted();
        Append(DiagnosticEventKind.MemoryPressure, "Memory Pressure", "OS memory warning");
    }

    public void Clear(bool includePersistedCrash = false)
    {
        lock (_gate)
        {
            _timeline.Clear();
            _exceptions.Clear();
            _apiCalls.Clear();
            _lastScreen = null;
            _lastUserAction = null;
            _lastSuccessfulApi = null;
            _lastAnr = null;
            _lowBatteryNoted = false;
            _lowStorageNoted = false;
            _memoryNoted = false;
            if (includePersistedCrash)
            {
                _lastCrash = null;
                _persistence.ClearCrash();
            }

            if (_started)
            {
                PersistSessionLocked(cleanShutdown: false);
            }
        }
    }

    public void OnNativeCrash(string type, string message, string? stackTrace)
    {
        var exception = new Exception($"{type}: {message}")
        {
            Source = type
        };
        OnCapturedException(exception, isUnhandled: true);
    }

    public void OnMemoryWarning() => NotifyMemoryWarning();

    internal void RecordCrash(Exception exception) =>
        RecordException(exception, isUnhandled: true, persistCrash: true, null);

    internal void SimulateAnr(TimeSpan duration) => OnAnr(duration);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            Stop();
        }
        catch
        {
            DisposeWatchers();
        }
    }

    void EnsureStarted()
    {
        if (!_started)
        {
            Start();
        }
    }

    void RecoverLocked()
    {
        var crash = _persistence.LoadCrash();
        if (crash is not null)
        {
            _lastCrash = new CrashRecord(
                crash.OccurredAt,
                crash.SessionId,
                crash.Exception.ToRecord(),
                crash.Timeline.Select(static entry => entry.ToEntry()).ToArray());
        }

        var previous = _persistence.LoadSession();
        if (previous is { CleanShutdown: false, Timeline.Count: > 0 } && _lastCrash is null)
        {
            _lastCrash = new CrashRecord(
                previous.Timeline[^1].Timestamp,
                previous.SessionId,
                new ExceptionRecord(
                    previous.Timeline[^1].Timestamp,
                    "UnexpectedTermination",
                    "The previous session ended without a clean shutdown.",
                    null,
                    isUnhandled: true),
                previous.Timeline.Select(static entry => entry.ToEntry()).ToArray());
        }
    }

    void OnCapturedException(Exception exception, bool isUnhandled) =>
        RecordException(exception, isUnhandled, persistCrash: isUnhandled, null);

    void RecordException(
        Exception exception,
        bool isUnhandled,
        bool persistCrash,
        IReadOnlyDictionary<string, string>? properties)
    {
        var record = new ExceptionRecord(
            _clock.UtcNow,
            exception.GetType().FullName ?? exception.GetType().Name,
            exception.Message,
            _options.IncludeStackTraces ? exception.ToString() : null,
            isUnhandled,
            properties);

        CrashRecord? crash = null;
        TimelineEntry entry;
        lock (_gate)
        {
            Remember(_exceptions, record, _options.MaxRecentExceptions);
            entry = AddLocked(
                isUnhandled ? DiagnosticEventKind.Crash : DiagnosticEventKind.Exception,
                isUnhandled ? "Crash" : "Exception",
                $"{record.Type}: {record.Message}",
                properties);

            if (persistCrash)
            {
                crash = new CrashRecord(_clock.UtcNow, _sessionId, record, _timeline.Snapshot());
                _lastCrash = crash;
                _persistence.SaveCrash(new PersistedCrash
                {
                    OccurredAt = crash.OccurredAt,
                    SessionId = crash.SessionId,
                    Exception = PersistedException.From(record),
                    Timeline = crash.TimelineBeforeCrash.Select(PersistedEntry.From).ToList()
                });
            }

            PersistSessionLocked(cleanShutdown: false);
        }

        RaiseTimeline(entry);
        if (crash is not null)
        {
            CrashCaptured?.Invoke(this, new CrashCapturedEventArgs(crash));
            _options.Events.OnCrashCaptured?.Invoke(crash);
        }
    }

    void OnAnr(TimeSpan duration)
    {
        AnrRecord anr;
        lock (_gate)
        {
            var snapshot = _timeline.Snapshot();
            anr = new AnrRecord(_clock.UtcNow, duration, _lastScreen, _lastUserAction, snapshot);
            _lastAnr = anr;
        }

        Append(DiagnosticEventKind.Anr, "ANR", $"Main thread frozen for {duration.TotalSeconds:0.0}s");
        AnrDetected?.Invoke(this, new AnrDetectedEventArgs(anr));
        _options.Events.OnAnrDetected?.Invoke(anr);
    }

    void OnNetworkChanged(bool online, string? profiles)
    {
        if (online)
        {
            Append(DiagnosticEventKind.NetworkRestored, "Network Restored", profiles);
        }
        else
        {
            Append(DiagnosticEventKind.NetworkLost, "Network Lost", profiles);
        }
    }

    DeviceSnapshot CollectDevice()
    {
        var collected = _environment.Collect();
        var builder = new DeviceSnapshotBuilder
        {
            Manufacturer = collected.Manufacturer,
            Model = collected.Model,
            Platform = collected.Platform,
            OsVersion = collected.OsVersion,
            Idiom = collected.Idiom,
            IsVirtualDevice = collected.IsVirtualDevice,
            AppVersion = collected.AppVersion,
            AppBuild = collected.AppBuild,
            PackageName = collected.PackageName,
            BatteryPercent = collected.BatteryPercent,
            BatteryState = collected.BatteryState,
            EnergySaverOn = collected.EnergySaverOn,
            FreeStorageBytes = collected.FreeStorageBytes,
            TotalStorageBytes = collected.TotalStorageBytes,
            AvailableMemoryBytes = collected.AvailableMemoryBytes,
            TotalMemoryBytes = collected.TotalMemoryBytes,
            AppUsedMemoryBytes = collected.AppUsedMemoryBytes,
            IsLowMemory = collected.IsLowMemory,
            MemoryPressure = collected.MemoryPressure,
            HasNetwork = collected.HasNetwork,
            HasInternet = collected.HasInternet,
            IsExpensive = collected.IsExpensive,
            IsConstrained = collected.IsConstrained,
            IsAirplaneMode = collected.IsAirplaneMode,
            DebuggerAttached = collected.DebuggerAttached
        };
        builder.ConnectionProfiles.AddRange(collected.ConnectionProfiles);
        _platform.Enrich(builder);
        return builder.Build();
    }

    void NoteEnvironment(DeviceSnapshot device)
    {
        if (_options.LowBatteryPercent > 0 &&
            device.BatteryPercent is { } battery &&
            battery <= _options.LowBatteryPercent &&
            !_lowBatteryNoted)
        {
            _lowBatteryNoted = true;
            Append(DiagnosticEventKind.Battery, "Low Battery", $"{battery:0}%");
        }

        if (_options.LowStorageMegabytes > 0 &&
            device.FreeStorageBytes is { } free &&
            free <= _options.LowStorageMegabytes * 1024L * 1024L &&
            !_lowStorageNoted)
        {
            _lowStorageNoted = true;
            Append(DiagnosticEventKind.Storage, "Low Storage", $"{free / (1024L * 1024L)} MB free");
        }

        if (!_memoryNoted && device.MemoryPressure is MemoryPressureKind.Warning or MemoryPressureKind.Critical)
        {
            _memoryNoted = true;
            Append(DiagnosticEventKind.MemoryPressure, "Memory Pressure", device.MemoryPressure.ToString());
        }
    }

    void Append(
        DiagnosticEventKind kind,
        string title,
        string? detail = null,
        IReadOnlyDictionary<string, string>? properties = null)
    {
        TimelineEntry entry;
        lock (_gate)
        {
            entry = AddLocked(kind, title, detail, properties);
            PersistSessionLocked(cleanShutdown: false);
        }

        RaiseTimeline(entry);
    }

    TimelineEntry AddLocked(
        DiagnosticEventKind kind,
        string title,
        string? detail,
        IReadOnlyDictionary<string, string>? properties)
    {
        var entry = new TimelineEntry(_clock.UtcNow, kind, title, detail, properties);
        _timeline.Add(entry);
        return entry;
    }

    void PersistSessionLocked(bool cleanShutdown)
    {
        _persistence.SaveSession(new PersistedSession
        {
            SessionId = _sessionId,
            StartedAt = _startedAt,
            CleanShutdown = cleanShutdown,
            LastScreen = _lastScreen,
            LastUserAction = _lastUserAction,
            LastSuccessfulApi = _lastSuccessfulApi is null ? null : PersistedApiCall.From(_lastSuccessfulApi),
            Timeline = _timeline.Snapshot().Select(PersistedEntry.From).ToList(),
            Exceptions = _exceptions.Select(PersistedException.From).ToList(),
            ApiCalls = _apiCalls.Select(PersistedApiCall.From).ToList()
        });
    }

    void RaiseTimeline(TimelineEntry entry)
    {
        TimelineUpdated?.Invoke(this, new TimelineUpdatedEventArgs(entry));
        _options.Events.OnTimelineUpdated?.Invoke(entry);
    }

    void DisposeWatchers()
    {
        _anrWatchdog.Dispose();
        _crashGuard.Dispose();
        _networkWatcher.Dispose();
        _navigationWatcher.Dispose();
        _platformWatch?.Dispose();
        _platformWatch = null;
    }

    static void Remember<T>(List<T> list, T item, int max)
    {
        list.Add(item);
        if (list.Count > max)
        {
            list.RemoveRange(0, list.Count - max);
        }
    }

    static bool IsNetworkError(string? error) =>
        error is not null &&
        (error.Contains("HttpRequestException", StringComparison.OrdinalIgnoreCase) ||
         error.Contains("timed out", StringComparison.OrdinalIgnoreCase) ||
         error.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
         error.Contains("network", StringComparison.OrdinalIgnoreCase) ||
         error.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
         error.Contains("unreachable", StringComparison.OrdinalIgnoreCase));

    static Dictionary<string, string> ToProperties(ApiCallRecord call)
    {
        var properties = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["method"] = call.Method,
            ["url"] = call.Url,
            ["status"] = call.Status.ToString(),
            ["attempt"] = call.Attempt.ToString()
        };
        if (call.StatusCode is { } code)
        {
            properties["statusCode"] = code.ToString();
        }

        if (call.Duration is { } duration)
        {
            properties["durationMs"] = ((int)duration.TotalMilliseconds).ToString();
        }

        if (!string.IsNullOrWhiteSpace(call.Error))
        {
            properties["error"] = call.Error;
        }

        return properties;
    }
}
