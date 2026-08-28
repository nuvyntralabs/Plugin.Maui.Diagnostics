namespace Plugin.Maui.Diagnostics.Tests;

public sealed class ReportTests
{
    [Fact]
    public async Task Report_includes_device_session_and_timeline()
    {
        var (diagnostics, clock, environment, _) = Harness.Create();
        diagnostics.Start();
        diagnostics.TrackScreen("Home");
        diagnostics.TrackUserAction("Open cart");
        diagnostics.TrackApiCall(new ApiCallRecord(clock.UtcNow, "GET", "https://api.shop/cart", ApiCallStatus.Succeeded, 200));

        var report = await diagnostics.GenerateReportAsync();

        Assert.Equal("Pixel Test", report.Device.Model);
        Assert.Equal("16", report.Device.OsVersion);
        Assert.Equal("1.2.3", report.Device.AppVersion);
        Assert.Equal(82, report.Device.BatteryPercent);
        Assert.Equal("Home", report.Session.LastScreen);
        Assert.Equal("Open cart", report.Session.LastUserAction);
        Assert.Equal("https://api.shop/cart", report.Session.LastSuccessfulApiCall?.Url);
        Assert.True(report.Session.IsInForeground);
        Assert.Contains(report.Timeline, entry => entry.Title == "Home");
        Assert.False(string.IsNullOrWhiteSpace(report.FormatTimeline()));
        Assert.Same(environment.Snapshot.Model, report.Device.Model);
    }

    [Fact]
    public async Task Background_is_reflected_on_the_session()
    {
        var (diagnostics, _, _, _) = Harness.Create();
        diagnostics.Start();
        diagnostics.NotifyBackground();

        var report = await diagnostics.GenerateReportAsync();

        Assert.False(report.Session.IsInForeground);
        Assert.Contains(report.Timeline, entry => entry.Kind == DiagnosticEventKind.AppBackground);
    }

    [Fact]
    public void Clear_drops_live_breadcrumbs_but_keeps_crash_until_asked()
    {
        var (diagnostics, _, _, _) = Harness.Create();
        diagnostics.Start();
        diagnostics.TrackEvent("Keep me");
        diagnostics.RecordCrash(new InvalidOperationException("fatal"));

        diagnostics.Clear();
        Assert.Empty(diagnostics.GetTimeline());
        Assert.NotNull(diagnostics.LastCrash);

        diagnostics.Clear(includePersistedCrash: true);
        Assert.Null(diagnostics.LastCrash);
    }
}
