namespace MRX.UI.ErrorsAutoWiring.Tests.Http;

public class MockHttpClientMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(handler(request));
    }
}