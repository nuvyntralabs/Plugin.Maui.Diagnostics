using Plugin.Maui.Diagnostics;

namespace Plugin.Maui.Diagnostics.Sample;

public partial class MainPage : ContentPage
{
    readonly IMauiDiagnostics _diagnostics;
    readonly OrderPage _orderPage;

    public MainPage(IMauiDiagnostics diagnostics, OrderPage orderPage)
    {
        InitializeComponent();
        _diagnostics = diagnostics;
        _orderPage = orderPage;
        _diagnostics.TimelineUpdated += (_, _) => MainThread.BeginInvokeOnMainThread(RefreshTimeline);
        RefreshTimeline();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _diagnostics.TrackScreen("Home");
        RefreshTimeline();
    }

    void OnLoginClicked(object? sender, EventArgs e)
    {
        _diagnostics.TrackUserAction("Tap Login");
        _diagnostics.TrackEvent("Login Success");
    }

    async void OnOrderClicked(object? sender, EventArgs e)
    {
        _diagnostics.TrackUserAction("Open Order Screen");
        await Navigation.PushAsync(_orderPage);
    }

    void OnApiSuccessClicked(object? sender, EventArgs e)
    {
        _diagnostics.TrackUserAction("Tap Place Order");
        _diagnostics.TrackApiCall(new ApiCallRecord(
            DateTimeOffset.UtcNow,
            "POST",
            "https://api.shop/orders?session=demo",
            ApiCallStatus.Started));
        _diagnostics.TrackApiCall(new ApiCallRecord(
            DateTimeOffset.UtcNow,
            "POST",
            "https://api.shop/orders?session=demo",
            ApiCallStatus.Succeeded,
            201,
            TimeSpan.FromMilliseconds(120)));
    }

    void OnNetworkLostClicked(object? sender, EventArgs e)
    {
        _diagnostics.TrackNetworkFailure("Network Lost");
        _diagnostics.TrackEvent("Network Lost");
    }

    void OnNetworkRestoredClicked(object? sender, EventArgs e) =>
        _diagnostics.TrackEvent("Network Restored");

    void OnApiRetryClicked(object? sender, EventArgs e)
    {
        _diagnostics.TrackApiCall(new ApiCallRecord(
            DateTimeOffset.UtcNow,
            "POST",
            "https://api.shop/orders",
            ApiCallStatus.Retried,
            attempt: 2));
        _diagnostics.TrackApiCall(new ApiCallRecord(
            DateTimeOffset.UtcNow,
            "POST",
            "https://api.shop/orders",
            ApiCallStatus.Failed,
            503,
            TimeSpan.FromSeconds(2),
            "Service Unavailable",
            2));
    }

    void OnExceptionClicked(object? sender, EventArgs e) =>
        _diagnostics.TrackException(new InvalidOperationException("Payment declined"));

    void OnAnrClicked(object? sender, EventArgs e)
    {
        _diagnostics.TrackUserAction("Tap Freeze UI");
        Thread.Sleep(TimeSpan.FromSeconds(6));
    }

    void OnCrashClicked(object? sender, EventArgs e)
    {
        _diagnostics.TrackUserAction("Tap Crash");
        throw new InvalidOperationException("Simulated crash — relaunch to see the recovered timeline.");
    }

    async void OnReportClicked(object? sender, EventArgs e)
    {
        try
        {
            var report = await MauiDiagnostics.GenerateReportAsync();
            ReportLabel.Text =
                $"Device {report.Device.Manufacturer} {report.Device.Model}{Environment.NewLine}" +
                $"OS {report.Device.Platform} {report.Device.OsVersion}{Environment.NewLine}" +
                $"App {report.Device.AppVersion} ({report.Device.AppBuild}){Environment.NewLine}" +
                $"Battery {report.Device.BatteryPercent:0}% {report.Device.BatteryState}{Environment.NewLine}" +
                $"Memory {report.Device.MemoryPressure}  Storage free {FormatBytes(report.Device.FreeStorageBytes)}{Environment.NewLine}" +
                $"Last screen: {report.Session.LastScreen}{Environment.NewLine}" +
                $"Last action: {report.Session.LastUserAction}{Environment.NewLine}" +
                $"Last API: {report.Session.LastSuccessfulApiCall?.Method} {report.Session.LastSuccessfulApiCall?.Url}{Environment.NewLine}" +
                $"Last crash: {report.LastCrash?.Exception.Type ?? "none"}{Environment.NewLine}" +
                $"Last ANR: {(report.LastAnr is { } anr ? $"{anr.Duration.TotalSeconds:0.0}s" : "none")}";
            RefreshTimeline();
        }
        catch (Exception ex)
        {
            ReportLabel.Text = ex.Message;
        }
    }

    void RefreshTimeline()
    {
        var live = _diagnostics.FormatTimeline();
        var crash = _diagnostics.LastCrash?.FormatTimeline();
        TimelineLabel.Text = string.IsNullOrWhiteSpace(crash)
            ? string.IsNullOrWhiteSpace(live) ? "No breadcrumbs yet." : live
            : $"Recovered crash timeline{Environment.NewLine}{crash}{Environment.NewLine}{Environment.NewLine}This session{Environment.NewLine}{live}";
    }

    static string FormatBytes(long? bytes) =>
        bytes is null ? "n/a" : $"{bytes.Value / (1024d * 1024d):0} MB";
}
