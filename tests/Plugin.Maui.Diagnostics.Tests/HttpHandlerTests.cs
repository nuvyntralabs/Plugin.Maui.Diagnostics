namespace Plugin.Maui.Diagnostics.Tests;

public sealed class HttpHandlerTests
{
    [Fact]
    public async Task Records_request_and_success()
    {
        var (diagnostics, _, _, _) = Harness.Create();
        diagnostics.Start();

        using var handler = new DiagnosticsDelegatingHandler(new StubHandler(HttpStatusCode.OK), diagnostics);
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://api.shop/cart?token=abc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(diagnostics.GetTimeline(), entry => entry.Kind == DiagnosticEventKind.ApiRequest);
        Assert.Contains(diagnostics.GetTimeline(), entry => entry.Kind == DiagnosticEventKind.ApiSuccess);
        Assert.Equal("https://api.shop/cart", diagnostics.LastSuccessfulApiCall?.Url);
    }

    [Fact]
    public async Task Records_http_failure()
    {
        var (diagnostics, _, _, _) = Harness.Create();
        diagnostics.Start();

        using var handler = new DiagnosticsDelegatingHandler(new StubHandler(HttpStatusCode.BadGateway), diagnostics);
        using var client = new HttpClient(handler);

        await client.GetAsync("https://api.shop/pay");

        Assert.Contains(diagnostics.GetTimeline(), entry => entry.Kind == DiagnosticEventKind.ApiFailure);
        Assert.Null(diagnostics.LastSuccessfulApiCall);
    }

    sealed class StubHandler(HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status)
            {
                RequestMessage = request,
                ReasonPhrase = status.ToString()
            });
    }
}
