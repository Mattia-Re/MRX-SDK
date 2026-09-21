using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MRX.Core.Common.Features;
using MRX.Core.ModelBinding;
using MRX.FeatureDiscovery.Common;

namespace MRX.FeatureDiscovery.Tests;

public class FeatureDiscoveryTests(FeatureDiscoveryTestFixture fixture) : IClassFixture<FeatureDiscoveryTestFixture>
{
    private readonly HttpClient _client = fixture.Client;

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Probe_Endpoints_DiscoversFeatures()
    {
        // Arrange & Act

        HttpRequestMessage disambiguateReq = new(HttpMethod.Options, "disambiguate");
        disambiguateReq.Headers.Add("X-Probe-Method", "POST");
        HttpResponseMessage disambiguateRes = await _client.SendAsync(disambiguateReq);
        disambiguateRes.EnsureSuccessStatusCode();

        FeatureDiscoveryResponse disambiguateDiscovery =
            (await disambiguateRes.Content.ReadFromJsonAsync<FeatureDiscoveryResponse>())!;

        HttpRequestMessage noFeatureReq = new(HttpMethod.Options, "no-features");
        noFeatureReq.Headers.Add("X-Probe-Method", "POST");
        HttpResponseMessage noFeatureRes = await _client.SendAsync(noFeatureReq);
        noFeatureRes.EnsureSuccessStatusCode();

        FeatureDiscoveryResponse noFeatureDiscovery =
            (await noFeatureRes.Content.ReadFromJsonAsync<FeatureDiscoveryResponse>())!;

        // Assert
        Assert.Contains(MrxEndpointFeatures.ErrorKeyDisambiguation, disambiguateDiscovery.Features);
        Assert.Empty(noFeatureDiscovery.Features);
    }

    [Fact]
    public async Task Probe_Endpoints_WithoutMethodHeader_ReturnsProblem()
    {
        // Arrange & Act
        HttpRequestMessage req = new(HttpMethod.Options, "disambiguate");
        HttpResponseMessage res = await _client.SendAsync(req);

        // Assert

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        ProblemDetails problem = (await res.Content.ReadFromJsonAsync<ProblemDetails>())!;
        bool hasErrorsExt = problem.Extensions.TryGetValue("errors", out object? errExt);

        Assert.True(hasErrorsExt);
        Assert.NotNull(errExt);

        Dictionary<string, CodedError[]>? errors =
            ((JsonElement)errExt).Deserialize<Dictionary<string, CodedError[]>>(_jsonOptions);

        Assert.NotNull(errors);

        CodedError[] keyErrors = errors
            .FirstOrDefault(kvp => kvp.Key.Equals("X-Probe-Method", StringComparison.OrdinalIgnoreCase)).Value;

        Assert.Single(keyErrors);
        Assert.Equal("InvalidValue", keyErrors[0].Code);
    }
}