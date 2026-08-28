namespace Plugin.Maui.Diagnostics.Tests;

public sealed class TrackingTests
{
    [Fact]
    public void Remembers_last_screen_action_and_successful_api()
    {
        var (diagnostics, clock, _, _) = Harness.Create();
        diagnostics.Start();

        diagnostics.TrackScreen("Checkout");
        diagnostics.TrackUserAction("Tap Pay");
        diagnostics.TrackApiCall(new ApiCallRecord(
            clock.UtcNow,
            "POST",
            "https://api.shop/pay?token=secret",
            ApiCallStatus.Succeeded,
            200,
            TimeSpan.FromMilliseconds(80)));

        Assert.Equal("Checkout", diagnostics.LastScreen);
        Assert.Equal("Tap Pay", diagnostics.LastUserAction);
        Assert.NotNull(diagnostics.LastSuccessfulApiCall);
        Assert.Equal("https://api.shop/pay", diagnostics.LastSuccessfulApiCall!.Url);
        Assert.True(diagnostics.LastSuccessfulApiCall.IsSuccess);
    }

    [Fact]
    public void Failed_api_does_not_replace_last_success()
    {
        var (diagnostics, clock, _, _) = Harness.Create();
        diagnostics.Start();

        diagnostics.TrackApiCall(new ApiCallRecord(clock.UtcNow, "GET", "https://api.shop/me", ApiCallStatus.Succeeded, 200));
        diagnostics.TrackApiCall(new ApiCallRecord(clock.UtcNow, "GET", "https://api.shop/me", ApiCallStatus.Failed, 500, error: "server"));

        Assert.Equal("https://api.shop/me", diagnostics.LastSuccessfulApiCall?.Url);
        Assert.Equal(ApiCallStatus.Succeeded, diagnostics.LastSuccessfulApiCall?.Status);
        Assert.Contains(diagnostics.GetTimeline(), entry => entry.Kind == DiagnosticEventKind.ApiFailure);
    }

    [Fact]
    public void Network_error_on_api_is_classified_as_network_failure()
    {
        var (diagnostics, clock, _, _) = Harness.Create();
        diagnostics.Start();

        diagnostics.TrackApiCall(new ApiCallRecord(
            clock.UtcNow,
            "GET",
            "https://api.shop/me",
            ApiCallStatus.Failed,
            error: "HttpRequestException: connection timed out"));

        Assert.Contains(diagnostics.GetTimeline(), entry => entry.Kind == DiagnosticEventKind.NetworkFailure);
    }

    [Fact]
    public void Static_facade_forwards_to_current()
    {
        var (diagnostics, _, _, _) = Harness.Create();
        MauiDiagnostics.SetDefault(diagnostics);

        MauiDiagnostics.TrackEvent("CheckoutStarted");
        MauiDiagnostics.TrackException(new InvalidOperationException("boom"));

        Assert.Contains(MauiDiagnostics.Current.GetTimeline(), entry => entry.Title == "CheckoutStarted");
        Assert.Contains(MauiDiagnostics.Current.GetTimeline(), entry => entry.Kind == DiagnosticEventKind.Exception);
    }

    [Fact]
    public void Anr_is_recorded_with_context()
    {
        var (diagnostics, _, _, _) = Harness.Create();
        diagnostics.Start();
        diagnostics.TrackScreen("Order Screen");
        diagnostics.TrackUserAction("Tap Checkout");

        AnrRecord? seen = null;
        diagnostics.AnrDetected += (_, e) => seen = e.Anr;
        diagnostics.SimulateAnr(TimeSpan.FromSeconds(6));

        Assert.NotNull(seen);
        Assert.Equal(TimeSpan.FromSeconds(6), seen!.Duration);
        Assert.Equal("Order Screen", seen.LastScreen);
        Assert.Equal("Tap Checkout", diagnostics.LastAnr?.LastUserAction);
        Assert.Contains(diagnostics.GetTimeline(), entry => entry.Kind == DiagnosticEventKind.Anr);
    }
}
