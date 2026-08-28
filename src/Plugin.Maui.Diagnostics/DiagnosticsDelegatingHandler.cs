namespace Plugin.Maui.Diagnostics;

/// <summary>
/// <see cref="HttpMessageHandler"/> that records API request, success, failure, and retry breadcrumbs.
/// </summary>
public sealed class DiagnosticsDelegatingHandler : DelegatingHandler
{
    readonly IMauiDiagnostics _diagnostics;
    readonly MauiDiagnosticsOptions _options;

    /// <summary>
    /// Creates a handler that writes to <see cref="MauiDiagnostics.Current"/>.
    /// </summary>
    public DiagnosticsDelegatingHandler()
        : this(MauiDiagnostics.Current, null)
    {
    }

    /// <summary>
    /// Creates a handler that writes to <paramref name="diagnostics"/>.
    /// </summary>
    public DiagnosticsDelegatingHandler(IMauiDiagnostics diagnostics, MauiDiagnosticsOptions? options = null)
    {
        _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _options = options ?? new MauiDiagnosticsOptions();
    }

    /// <summary>
    /// Creates a handler around an inner handler.
    /// </summary>
    public DiagnosticsDelegatingHandler(HttpMessageHandler innerHandler, IMauiDiagnostics? diagnostics = null, MauiDiagnosticsOptions? options = null)
        : base(innerHandler)
    {
        _diagnostics = diagnostics ?? MauiDiagnostics.Current;
        _options = options ?? new MauiDiagnosticsOptions();
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var started = DateTimeOffset.UtcNow;
        var method = request.Method.Method;
        var url = request.RequestUri?.ToString() ?? "";
        var attempt = 1;
        if (request.Options.TryGetValue(DiagnosticsHttp.AttemptKey, out var recorded) && recorded > 0)
        {
            attempt = recorded;
        }

        _diagnostics.TrackApiCall(new ApiCallRecord(started, method, url, ApiCallStatus.Started, attempt: attempt));
        if (attempt > 1)
        {
            _diagnostics.TrackApiCall(new ApiCallRecord(started, method, url, ApiCallStatus.Retried, attempt: attempt));
        }

        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var duration = DateTimeOffset.UtcNow - started;
            var success = IsSuccess((int)response.StatusCode);
            _diagnostics.TrackApiCall(new ApiCallRecord(
                DateTimeOffset.UtcNow,
                method,
                url,
                success ? ApiCallStatus.Succeeded : ApiCallStatus.Failed,
                (int)response.StatusCode,
                duration,
                success ? null : response.ReasonPhrase,
                attempt));
            return response;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            var duration = DateTimeOffset.UtcNow - started;
            _diagnostics.TrackApiCall(new ApiCallRecord(
                DateTimeOffset.UtcNow,
                method,
                url,
                ApiCallStatus.Failed,
                null,
                duration,
                ex.GetType().Name + ": " + ex.Message,
                attempt));
            throw;
        }
    }

    bool IsSuccess(int statusCode) =>
        _options.IsSuccessStatusCode?.Invoke(statusCode) ?? statusCode is >= 200 and <= 299;
}

/// <summary>
/// Keys used by <see cref="DiagnosticsDelegatingHandler"/> to correlate retries.
/// </summary>
public static class DiagnosticsHttp
{
    /// <summary>
    /// Set this on <see cref="HttpRequestMessage.Options"/> to mark a retry attempt (1-based).
    /// </summary>
    public static readonly HttpRequestOptionsKey<int> AttemptKey = new("Plugin.Maui.Diagnostics.Attempt");
}
