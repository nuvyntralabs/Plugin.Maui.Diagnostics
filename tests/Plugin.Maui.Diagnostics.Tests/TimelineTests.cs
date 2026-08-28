namespace Plugin.Maui.Diagnostics.Tests;

public sealed class TimelineTests
{
    [Fact]
    public void Formats_the_pre_crash_transcript()
    {
        var (diagnostics, clock, _, _) = Harness.Create();
        clock.Set(10, 21, 3);

        diagnostics.Start();
        clock.Set(10, 21, 5);
        diagnostics.TrackEvent("Login Success");
        clock.Set(10, 22, 11);
        diagnostics.TrackScreen("Order Screen");
        clock.Set(10, 22, 14);
        diagnostics.TrackApiCall(new ApiCallRecord(clock.UtcNow, "POST", "https://api.shop/orders", ApiCallStatus.Started));
        clock.Set(10, 22, 15);
        diagnostics.TrackNetworkFailure("offline");
        clock.Set(10, 22, 21);
        diagnostics.TrackEvent("Network Restored");
        clock.Set(10, 22, 24);
        diagnostics.TrackApiCall(new ApiCallRecord(clock.UtcNow, "POST", "https://api.shop/orders", ApiCallStatus.Retried, attempt: 2));
        clock.Set(10, 22, 26);
        diagnostics.TrackException(new InvalidOperationException("Payment declined"));

        var utc = TimeZoneInfo.Utc;
        var lines = TimelineFormatter.Format(diagnostics.GetTimeline(), utc).Split(Environment.NewLine);

        Assert.Contains(lines, line => line == "10:21:03  App Started");
        Assert.Contains(lines, line => line == "10:21:05  Login Success");
        Assert.Contains(lines, line => line == "10:22:11  Order Screen");
        Assert.Contains(lines, line => line == "10:22:14  API Request");
        Assert.Contains(lines, line => line == "10:22:15  Network Failure");
        Assert.Contains(lines, line => line == "10:22:21  Network Restored");
        Assert.Contains(lines, line => line == "10:22:24  API Retry");
        Assert.Contains(lines, line => line == "10:22:26  Exception");
    }

    [Fact]
    public void Caps_the_ring_buffer()
    {
        var (diagnostics, _, _, _) = Harness.Create(options => options.MaxTimelineEntries = 5);

        diagnostics.Start();
        for (var i = 0; i < 10; i++)
        {
            diagnostics.TrackEvent($"E{i}");
        }

        var titles = diagnostics.GetTimeline().Select(entry => entry.Title).ToArray();
        Assert.Equal(5, titles.Length);
        Assert.Equal(["E5", "E6", "E7", "E8", "E9"], titles);
    }

    [Fact]
    public void TrackEvent_requires_a_name()
    {
        var (diagnostics, _, _, _) = Harness.Create();
        Assert.Throws<ArgumentException>(() => diagnostics.TrackEvent(" "));
    }
}
