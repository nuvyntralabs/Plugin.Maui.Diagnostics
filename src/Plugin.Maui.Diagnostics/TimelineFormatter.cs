namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Formats a breadcrumb list as the "what happened before the crash" transcript.
/// </summary>
public static class TimelineFormatter
{
    /// <summary>
    /// Joins entries as local <c>HH:mm:ss  Title</c> lines.
    /// </summary>
    public static string Format(IEnumerable<TimelineEntry> entries, TimeZoneInfo? timeZone = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var zone = timeZone ?? TimeZoneInfo.Local;
        return string.Join(Environment.NewLine, entries.Select(entry => entry.Format(zone)));
    }
}
