using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using MRX.Core.Common;
using MRX.Core.ModelBinding;

namespace MRX.Core.Tests.ModelBinding;

public class ModelBindingBehaviorTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task InvalidRequestJson_ReturnsExpectedErrorResponse_WithKeyCasingAndWithoutDisambiguation()
    {
        // Arrange
        (StringContent body, string url) = GetRequestComponents("/no-disambiguate");

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync(url, body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Expected error keys are "ID" and "OBJ.NUM"
        // ID is expected to have 2 errors whereas OBJ.NUM only 1 belonging to the query because body will
        // already error on ID

        ProblemDetails problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.TryGetValue("errors", out Dictionary<string, CodedError[]>? errors);

        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.True(errors.ContainsKey("ID"));
        Assert.True(errors.ContainsKey("OBJ.NUM"));
        Assert.Equal(2, errors["ID"].Length);
        Assert.Single(errors["OBJ.NUM"]);
    }

    [Fact]
    public async Task InvalidRequestJson_ReturnsExpectedErrorResponse_WithKeyCasingAndWithDisambiguation()
    {
        // Arrange
        (StringContent body, string url) = GetRequestComponents("/disambiguate");

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync(url, body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Expected error keys are BODY.ID, QUERY.ID and QUERY.OBJ.NUM. Each is expected to have one error
        // BODY.OBJ.NUM is not expected because body will already fail on BODY.ID

        ProblemDetails problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.TryGetValue("errors", out Dictionary<string, CodedError[]>? errors);

        Assert.NotNull(errors);
        Assert.Equal(3, errors.Count);
        Assert.True(errors.ContainsKey("BODY.ID"));
        Assert.True(errors.ContainsKey("QUERY.ID"));
        Assert.True(errors.ContainsKey("QUERY.OBJ.NUM"));
        Assert.Single(errors["BODY.ID"]);
        Assert.Single(errors["QUERY.ID"]);
        Assert.Single(errors["QUERY.OBJ.NUM"]);
    }

    private static (StringContent body, string url) GetRequestComponents(string baseUrl)
    {
        const string invalidJson = """
                                   {
                                      "id": "str",
                                      "obj": {
                                          "num": "world"
                                      }
                                   }
                                   """;
        StringContent content = new(invalidJson, Encoding.UTF8, "application/json");
        Dictionary<string, string?> query = new()
        {
            { "id", "hello" },
            { "obj.num", "world" }
        };
        string url = QueryHelpers.AddQueryString(baseUrl, query);

        return (content, url);
    }
}