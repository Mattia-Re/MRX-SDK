using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using MRX.Core.ModelBinding;
using MRX.Json.Abstractions;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;
using MRX.Json.Reflection.PropertyAccess;
using MRX.Parsing.Reflection;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.Blazor.Tests.Models;
using MRX.UI.ErrorsAutoWiring.DependencyInjection;
using MRX.UI.ErrorsAutoWiring.Http;
using MRX.UI.ErrorsAutoWiring.ModelDiscovery;
using MRX.UI.ErrorsAutoWiring.Wiring;

namespace MRX.UI.ErrorsAutoWiring.Blazor.Tests;

/// <summary>
///     End-to-end tests proving that errors resolved by <see cref="ProblemDetailsHttpClient" /> from an RFC 9457
///     problem details response are actually rendered by a plain, unmodified Blazor <c>EditForm</c> - not just
///     added to the <see cref="EditContext" />, which is already covered elsewhere.
/// </summary>
public class ProblemDetailsHttpClientDisplayTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FailedCall_DisplaysErrors_ForNestedAndIndexedPathKeys(bool disambiguated)
    {
        // Arrange
        //
        // The Body and Query binding sources intentionally share the exact same model type, and the request
        // content built from them is identical regardless of the disambiguation state: only the mocked response
        // (key prefixes + the X-Disambiguated header) changes between the two theory cases.
        (SampleRequest body, SampleRequest query, HttpRequestMessage request) = BuildIdenticalRequest();

        Dictionary<string, CodedError[]> errors = new()
        {
            ["$"] = [new CodedError("RootInvalid", "Something is globally wrong")],
            [ResolveKey("obj.data", "body", disambiguated)] =
                [new CodedError("InvalidValue", "Body data is invalid")]
        };

        // Only when disambiguated can we safely target a field on the Query source without also hitting Body,
        // since both sources share the same model shape.
        if (disambiguated)
        {
            errors[ResolveKey("obj.cars[0].name", "query", disambiguated)] =
                [new CodedError("InvalidValue", "Query car name is invalid")];
        }

        var problem = new
        {
            type = "about:blank",
            status = 400,
            title = "Validation failed",
            detail = "One or more fields were invalid",
            errors
        };

        ProblemDetailsHttpClient client = CreateSut(problem, disambiguated);

        DataBindingSources sources = new() { Body = body, Query = query };
        EditContext editContext = new(sources);

        // Act
        await client.SendContextAwareAsync(editContext, request, CancellationToken.None);

        using TestContext ctx = new();
        IRenderedComponent<ErrorDisplayForm> cut = ctx.RenderComponent<ErrorDisplayForm>(parameters => parameters
            .Add(p => p.EditContext, editContext)
            .Add(p => p.Body, body)
            .Add(p => p.Query, query));

        // Assert

        // The root ("$") error is always displayed via the ValidationSummary
        Assert.Contains("Something is globally wrong", cut.Find("#root-summary").TextContent);

        // "obj.data" (or "body.obj.data" when disambiguated) always lands on Body.Obj.Data
        Assert.Contains("Body data is invalid", cut.Find("#body-obj-data-msg").TextContent);

        if (disambiguated)
        {
            // "body.obj.data" is scoped to Body only, so Query.Obj.Data must stay clean
            Assert.Empty(cut.FindAll("#query-obj-data-msg"));

            // "query.obj.cars[0].name" only lands on Query.Obj.Cars[0].Name
            Assert.Contains("Query car name is invalid", cut.Find("#query-obj-cars0-name-msg").TextContent);
            Assert.Empty(cut.FindAll("#body-obj-cars0-name-msg"));
        }
        else
        {
            // Without disambiguation, the plain "obj.data" key is ambiguous between the two identically-shaped
            // sources, so the SDK (correctly) displays the error on both matching fields.
            Assert.Contains("Body data is invalid", cut.Find("#query-obj-data-msg").TextContent);
            Assert.Empty(cut.FindAll("#body-obj-cars0-name-msg"));
            Assert.Empty(cut.FindAll("#query-obj-cars0-name-msg"));
        }
    }

    private static (SampleRequest Body, SampleRequest Query, HttpRequestMessage Request) BuildIdenticalRequest()
    {
        SampleRequest body = new()
        {
            Obj = new SampleObj
            {
                Data = "body-value",
                Cars = [new SampleCar { Name = "Body Car" }]
            }
        };

        SampleRequest query = new()
        {
            Obj = new SampleObj
            {
                Data = "query-value",
                Cars = [new SampleCar { Name = "Query Car" }]
            }
        };

        HttpRequestMessage request = new(HttpMethod.Post, "https://localhost/sample")
        {
            Content = JsonContent.Create(body)
        };

        return (body, query, request);
    }

    private static ProblemDetailsHttpClient CreateSut(object? problemBody, bool disambiguated,
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
                if (problemBody != null)
                {
                    res.Content = new StringContent(
                        JsonSerializer.Serialize(problemBody),
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

    private static string ResolveKey(string key, string sourceName, bool disambiguated)
    {
        return disambiguated
            ? $"{sourceName}.{key}"
            : key;
    }
}