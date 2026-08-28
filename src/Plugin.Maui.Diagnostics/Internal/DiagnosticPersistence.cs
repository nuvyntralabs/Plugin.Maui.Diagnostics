namespace Plugin.Maui.Diagnostics;

sealed class DiagnosticPersistence
{
    const string SessionFileName = "session.json";
    const string CrashFileName = "pending-crash.json";

    readonly string _directory;
    readonly bool _enabled;

    public DiagnosticPersistence(string directory, bool enabled)
    {
        _directory = directory;
        _enabled = enabled;
        if (_enabled)
        {
            Directory.CreateDirectory(_directory);
        }
    }

    public string DirectoryPath => _directory;

    public void SaveSession(PersistedSession session)
    {
        if (!_enabled)
        {
            return;
        }

        WriteAtomic(Path.Combine(_directory, SessionFileName), JsonSerializer.Serialize(session, DiagnosticsJsonContext.Default.PersistedSession));
    }

    public PersistedSession? LoadSession() =>
        Read<PersistedSession>(SessionFileName, DiagnosticsJsonContext.Default.PersistedSession);

    public void SaveCrash(PersistedCrash crash)
    {
        if (!_enabled)
        {
            return;
        }

        WriteAtomic(Path.Combine(_directory, CrashFileName), JsonSerializer.Serialize(crash, DiagnosticsJsonContext.Default.PersistedCrash));
    }

    public PersistedCrash? LoadCrash() =>
        Read<PersistedCrash>(CrashFileName, DiagnosticsJsonContext.Default.PersistedCrash);

    public void ClearCrash()
    {
        if (!_enabled)
        {
            return;
        }

        TryDelete(Path.Combine(_directory, CrashFileName));
    }

    public void ClearAll()
    {
        if (!_enabled)
        {
            return;
        }

        TryDelete(Path.Combine(_directory, SessionFileName));
        TryDelete(Path.Combine(_directory, CrashFileName));
    }

    T? Read<T>(string fileName, JsonTypeInfo<T> typeInfo) where T : class
    {
        if (!_enabled)
        {
            return null;
        }

        var path = Path.Combine(_directory, fileName);
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var json = File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize(json, typeInfo);
        }
        catch
        {
            return null;
        }
    }

    static void WriteAtomic(string path, string json)
    {
        var temp = path + ".tmp";
        try
        {
            File.WriteAllText(temp, json);
            File.Copy(temp, path, overwrite: true);
            TryDelete(temp);
        }
        catch
        {
            // Persistence must never take down the host app.
        }
    }

    static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best effort.
        }
    }
}
