namespace Plugin.Maui.Diagnostics;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(PersistedSession))]
[JsonSerializable(typeof(PersistedCrash))]
[JsonSerializable(typeof(PersistedEntry))]
[JsonSerializable(typeof(PersistedException))]
[JsonSerializable(typeof(PersistedApiCall))]
sealed partial class DiagnosticsJsonContext : JsonSerializerContext;
