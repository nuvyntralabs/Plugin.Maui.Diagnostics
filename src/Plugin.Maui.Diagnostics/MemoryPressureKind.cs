namespace Plugin.Maui.Diagnostics;

/// <summary>
/// Approximate process / system memory pressure.
/// </summary>
public enum MemoryPressureKind
{
    /// <summary>No reading is available.</summary>
    Unknown = 0,

    /// <summary>Memory looks healthy.</summary>
    Normal = 1,

    /// <summary>The OS or process is under pressure.</summary>
    Warning = 2,

    /// <summary>The process is close to being killed.</summary>
    Critical = 3
}
