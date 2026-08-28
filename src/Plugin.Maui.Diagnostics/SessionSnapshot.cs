namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Session context that support engineers ask for first.
/// </summary>
public sealed class SessionSnapshot
{
    /// <summary>Creates a session snapshot.</summary>
    public SessionSnapshot(
        string sessionId,
        DateTimeOffset startedAt,
        TimeSpan uptime,
        string? lastScreen,
        string? lastUserAction,
        ApiCallRecord? lastSuccessfulApiCall,
        bool isInForeground)
    {
        SessionId = sessionId;
        StartedAt = startedAt;
        Uptime = uptime;
        LastScreen = lastScreen;
        LastUserAction = lastUserAction;
        LastSuccessfulApiCall = lastSuccessfulApiCall;
        IsInForeground = isInForeground;
    }

    /// <summary>Id of the current process session.</summary>
    public string SessionId { get; }

    /// <summary>When this session started (UTC).</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>Elapsed time since session start.</summary>
    public TimeSpan Uptime { get; }

    /// <summary>Last tracked screen name.</summary>
    public string? LastScreen { get; }

    /// <summary>Last tracked user action.</summary>
    public string? LastUserAction { get; }

    /// <summary>Most recent successful API call.</summary>
    public ApiCallRecord? LastSuccessfulApiCall { get; }

    /// <summary>True when the app is believed to be in the foreground.</summary>
    public bool IsInForeground { get; }
}
