#if ANDROID || IOS
using Microsoft.Maui.Networking;
#endif

namespace Plugin.Maui.Diagnostics;

sealed class NetworkWatcher : IDisposable
{
    readonly Action<bool, string?> _onChanged;
    bool _started;
#if ANDROID || IOS
    bool? _hadInternet;
#endif

    public NetworkWatcher(Action<bool, string?> onChanged) => _onChanged = onChanged;

    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;
#if ANDROID || IOS
        try
        {
            _hadInternet = HasInternet();
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }
        catch
        {
            _started = false;
        }
#endif
    }

    public void Dispose()
    {
        if (!_started)
        {
            return;
        }

#if ANDROID || IOS
        try
        {
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
        }
        catch
        {
            // Host may already be tearing down.
        }
#endif
        _started = false;
    }

#if ANDROID || IOS
    void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var online = e.NetworkAccess is NetworkAccess.Internet or NetworkAccess.ConstrainedInternet;
        if (_hadInternet == online)
        {
            return;
        }

        _hadInternet = online;
        var profiles = string.Join(", ", e.ConnectionProfiles);
        _onChanged(online, string.IsNullOrWhiteSpace(profiles) ? null : profiles);
    }

    static bool? HasInternet()
    {
        try
        {
            return Connectivity.Current.NetworkAccess is NetworkAccess.Internet or NetworkAccess.ConstrainedInternet;
        }
        catch
        {
            return null;
        }
    }
#endif
}
