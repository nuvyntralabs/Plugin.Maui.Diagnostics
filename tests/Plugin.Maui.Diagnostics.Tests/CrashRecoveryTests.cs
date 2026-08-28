namespace Plugin.Maui.Diagnostics.Tests;

public sealed class CrashRecoveryTests
{
    [Fact]
    public async Task Next_launch_recovers_the_pre_crash_timeline()
    {
        var root = Directory.CreateTempSubdirectory("maui-diagnostics-crash-").FullName;
        var clock = new FakeClock();
        var options = Harness.TestOptions(root);

        var first = MauiDiagnostics.Create(options, clock, new FakeEnvironment(), new FakePlatform());
        first.Start();
        clock.Advance(TimeSpan.FromSeconds(2));
        first.TrackEvent("Login Success");
        clock.Advance(TimeSpan.FromMinutes(1));
        first.TrackScreen("Order Screen");
        clock.Advance(TimeSpan.FromSeconds(3));
        first.TrackApiCall(new ApiCallRecord(clock.UtcNow, "POST", "https://api.shop/orders", ApiCallStatus.Started));
        clock.Advance(TimeSpan.FromSeconds(1));
        first.TrackNetworkFailure("Network Lost");
        clock.Advance(TimeSpan.FromSeconds(6));
        first.TrackEvent("Network Restored");
        clock.Advance(TimeSpan.FromSeconds(3));
        first.TrackApiCall(new ApiCallRecord(clock.UtcNow, "POST", "https://api.shop/orders", ApiCallStatus.Retried, attempt: 2));
        clock.Advance(TimeSpan.FromSeconds(2));
        first.RecordCrash(new InvalidOperationException("Payment pipeline exploded"));
        first.Dispose();

        var second = MauiDiagnostics.Create(options, clock, new FakeEnvironment(), new FakePlatform());
        second.Start();
        var report = await second.GenerateReportAsync();

        Assert.NotNull(report.LastCrash);
        Assert.Equal("InvalidOperationException", report.LastCrash!.Exception.Type.Split('.').Last());
        Assert.Contains("Payment pipeline exploded", report.LastCrash.Exception.Message);

        var transcript = report.FormatWhatHappened();
        Assert.Contains("App Started", transcript);
        Assert.Contains("Login Success", transcript);
        Assert.Contains("Order Screen", transcript);
        Assert.Contains("API Request", transcript);
        Assert.Contains("Network Failure", transcript);
        Assert.Contains("Network Restored", transcript);
        Assert.Contains("API Retry", transcript);
        Assert.Contains("Crash", transcript);
    }

    [Fact]
    public async Task Unclean_shutdown_without_crash_file_is_treated_as_termination()
    {
        var root = Directory.CreateTempSubdirectory("maui-diagnostics-term-").FullName;
        var options = Harness.TestOptions(root);

        var first = MauiDiagnostics.Create(options, new FakeClock(), new FakeEnvironment(), new FakePlatform());
        first.Start();
        first.TrackEvent("Still working");
        // Leave the session file marked unclean — no Stop() — as if the process died.

        var second = MauiDiagnostics.Create(options, new FakeClock(), new FakeEnvironment(), new FakePlatform());
        second.Start();
        var report = await second.GenerateReportAsync();

        Assert.NotNull(report.LastCrash);
        Assert.Equal("UnexpectedTermination", report.LastCrash!.Exception.Type);
        Assert.Contains(report.LastCrash.TimelineBeforeCrash, entry => entry.Title == "Still working");
    }

    [Fact]
    public async Task Clean_stop_does_not_look_like_a_crash()
    {
        var root = Directory.CreateTempSubdirectory("maui-diagnostics-clean-").FullName;
        var options = Harness.TestOptions(root);

        var first = MauiDiagnostics.Create(options, new FakeClock(), new FakeEnvironment(), new FakePlatform());
        first.Start();
        first.TrackEvent("Done");
        first.Stop();

        var second = MauiDiagnostics.Create(options, new FakeClock(), new FakeEnvironment(), new FakePlatform());
        second.Start();
        var report = await second.GenerateReportAsync();

        Assert.Null(report.LastCrash);
    }
}
