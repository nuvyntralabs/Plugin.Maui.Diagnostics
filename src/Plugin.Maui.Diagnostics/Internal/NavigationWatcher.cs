#if ANDROID || IOS
using Microsoft.Maui.Controls;
#endif

namespace Plugin.Maui.Diagnostics;

sealed class NavigationWatcher : IDisposable
{
    readonly Action<string> _onScreen;
    bool _started;

    public NavigationWatcher(Action<string> onScreen) => _onScreen = onScreen;

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
            if (Application.Current is { } app)
            {
                app.PageAppearing += OnPageAppearing;
            }
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
            if (Application.Current is { } app)
            {
                app.PageAppearing -= OnPageAppearing;
            }
        }
        catch
        {
            // Host may already be tearing down.
        }
#endif
        _started = false;
    }

#if ANDROID || IOS
    void OnPageAppearing(object? sender, Page page)
    {
        if (page is NavigationPage or FlyoutPage or TabbedPage or Shell)
        {
            return;
        }

        var title = !string.IsNullOrWhiteSpace(page.Title) ? page.Title : page.GetType().Name;
        _onScreen(title);
    }
#endif
}
