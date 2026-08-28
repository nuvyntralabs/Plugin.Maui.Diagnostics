namespace Plugin.Maui.Diagnostics;

internal interface IClock
{
    DateTimeOffset UtcNow { get; }
}
