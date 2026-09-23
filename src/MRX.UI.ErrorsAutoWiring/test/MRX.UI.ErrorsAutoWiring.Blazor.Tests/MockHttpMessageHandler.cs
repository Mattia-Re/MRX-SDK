namespace MRX.UI.ErrorsAutoWiring.Blazor.Tests;

/// <summary>
///     A minimal <see cref="HttpMessageHandler" /> that always returns a response built by the supplied factory,
///     used to mock the HTTP transport wired into <see cref="Http.ProblemDetailsHttpClient" />.
/// </summary>
public class MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(responseFactory(request));
    }
}