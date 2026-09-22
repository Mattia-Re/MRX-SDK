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
using MRX.Json.Abstractions;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;
using MRX.Json.Reflection.PropertyAccess;
using MRX.Parsing.Reflection;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.DependencyInjection;
using MRX.UI.ErrorsAutoWiring.Http;
using MRX.UI.ErrorsAutoWiring.ModelDiscovery;
using MRX.UI.ErrorsAutoWiring.Wiring;

namespace MRX.UI.ErrorsAutoWiring.Tests.Http;

public class ProblemDetailsHttpClientTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProblemDetailsHttpClient_WiresErrorsCorrectly(bool disambiguated)
    {
        // Arrange

        var problem = new
        {
            type = "about:blank",
            status = 400,
            title = "Validation failed",
            detail = "One or more fields were invalid",
            errors = new Dictionary<string, CodedError[]>
            {
                { "$", [new CodedError("InvalidValue", "Resource is immutable")] },
                {
                    ResolveKeyFormat("id", "body", disambiguated),
                    [new CodedError("InvalidValue", "This value is not accepted")]
                }
            }
        };

        ProblemDetailsHttpClient client = CreateSut(problem, disambiguated);
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

        //Act

        await client.SendContextAwareAsync(editContext, req, CancellationToken.None);
        IEnumerable<string> messages = editContext.GetValidationMessages();

        // Assert

        Assert.Equal(2, messages.Count());
    }

    private static ProblemDetailsHttpClient CreateSut(object? body, bool disambiguated,
        string contentType = "application/problem+json")
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

                res.Headers.Add("X-Disambiguated", disambiguated.ToString());

                return res;
            });

        HttpClient httpClient = new(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost")
        };

        ProblemDetailsHttpClientOptions options = new();
        IJsonPathWalkerFactory walkerFactory = new JsonPathWalkerFactory();
        IModelPropertyAccessorProvider accessorProvider = new ModelPropertyAccessorProvider([
            new StringNodeAccessor(),
            new ArrayElementAccessor(),
            new CustomIndexerArrayAccessor(new NumericParser())
        ]);
        IModelKeyPathVisitor modelVisitor = new ModelKeyPathVisitor(walkerFactory, accessorProvider);
        IErrorMappingFactory errorMappingFactory = new DefaultErrorMappingFactory(modelVisitor);

        new ProblemDetailsHttpClientOptionsSetup(errorMappingFactory).Configure(options);

        return new ProblemDetailsHttpClient(httpClient, Options.Create(options));
    }

    private static string ResolveKeyFormat(string key, string sourceName, bool disambiguated)
    {
        return disambiguated
            ? $"{sourceName}.{key}"
            : key;
    }
}