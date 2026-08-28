namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Outcome of a tracked API call.
/// </summary>
public enum ApiCallStatus
{
    /// <summary>The request was sent and is still in flight.</summary>
    Started = 0,

    /// <summary>The response was treated as success.</summary>
    Succeeded = 1,

    /// <summary>The response or transport failed.</summary>
    Failed = 2,

    /// <summary>The caller is retrying the same logical request.</summary>
    Retried = 3
}
