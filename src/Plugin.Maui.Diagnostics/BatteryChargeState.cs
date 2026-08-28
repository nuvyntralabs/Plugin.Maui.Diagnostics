namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Battery charging state captured in a device snapshot.
/// </summary>
public enum BatteryChargeState
{
    /// <summary>The OS did not report a state.</summary>
    Unknown = 0,

    /// <summary>Currently charging.</summary>
    Charging = 1,

    /// <summary>On battery and discharging.</summary>
    Discharging = 2,

    /// <summary>Full.</summary>
    Full = 3,

    /// <summary>Plugged in but not charging.</summary>
    NotCharging = 4,

    /// <summary>No battery is present.</summary>
    NotPresent = 5
}
