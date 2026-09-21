using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using MRX.Core.ModelBinding;
using MRX.Core.TestHost.Controllers;
using MRX.UI.ErrorsAutoWiring.DependencyInjection;
using MRX.UI.ErrorsAutoWiring.Http;

namespace MRX.UI.ErrorsAutoWiring.Tests.Http;

public class ProblemDetailsHttpClientTests
{
    [Fact]
    public async Task ProblemDetailsHttpClient_WiresErrorsCorrectly()
    {
        var problem = new
        {
            type = "about:blank",
            status = 400,
            title = "Validation failed",
            detail = "One or more fields were invalid",
            errors = new Dictionary<string, CodedError[]>
            {
                { "$", [new CodedError("InvalidValue", "Resource is immutable")] }
            }
        };

        ProblemDetailsHttpClient client = CreateSut(problem);
        DataBindingSources sources = new()
        {
            Body = new ModelBindingTestFixtureController.MyRequest(
                100,
                new ModelBindingTestFixtureController.MyObject(200))
        };
        EditContext editContext = new(sources);

        HttpRequestMessage req = new(HttpMethod.Post, "/no-disambiguate")
        {
            Content = JsonContent.Create(sources.Body)
        };

        await client.SendContextAwareAsync(editContext, req, CancellationToken.None);
    }

    private static ProblemDetailsHttpClient CreateSut(object? body, string contentType = "application/problem+json")
    {
        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() =>
            {
                HttpResponseMessage res = new(HttpStatusCode.BadRequest);
                if (body != null)
                {
                    res.Content = new StringContent(
                        JsonSerializer.Serialize(body),
                        Encoding.UTF8,
                        contentType);
                }

                return res;
            });

        HttpClient httpClient = new(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost")
        };

        ProblemDetailsHttpClientOptions options = new();
        new ProblemDetailsHttpClientOptionsSetup().Configure(options);

        return new ProblemDetailsHttpClient(httpClient, Options.Create(options));
    }
}