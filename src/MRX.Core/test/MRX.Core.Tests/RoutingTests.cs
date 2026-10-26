using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MRX.Core.Common.ModelBinding;
using MRX.Core.Routing.Abstractions;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.Testing;
using MRX.Core.Routing;
using MRX.Core.Tests.Setup;

namespace MRX.Core.Tests;

public class RoutingTests(TestFactory factory) : IClassFixture<TestFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    public async Task Internal_rewrite_sets_new_endpoint_for_any_compatible_http_method(string method)
    {
        HttpRequestMessage req = new(new HttpMethod(method), "/internal-reexecute");
        HttpResponseMessage res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("reexecute", factory.Capture.Last!.Metadata.OfType<IRouteDiagnosticsMetadata>().First().Route);
    }

    [Fact]
    public async Task Internal_rewrite_fails_when_target_does_not_accept_the_http_method()
    {
        // The rewrite target only accepts GET and POST
        HttpRequestMessage req = new(HttpMethod.Delete, "/internal-reexecute");
        HttpResponseMessage res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, res.StatusCode);
    }

    [Fact]
    public async Task Recursive_rewrite_is_caught()
    {
        HttpRequestMessage req = new(HttpMethod.Get, "/recursive-internal-reexecute");
        req.Headers.Accept.ParseAdd("text/plain");
        HttpResponseMessage res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);

        // The developer exception page names the exception that was thrown
        Assert.Contains(nameof(ReExecutionTooDeepException), await res.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Rewrite_does_not_carry_validator_state_over_into_the_new_execution()
    {
        // The first action validates a parameter, then the request is rewritten to an action that fails validation
        HttpResponseMessage res = await _client.PostAsync("/validated-reexecute?page=1&id=hello&obj.num=world",
            new StringContent("", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        ProblemDetails problem = (await res.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.TryGetValue("errors", out Dictionary<string, CodedError[]>? errors);

        Assert.NotNull(errors);
        Assert.DoesNotContain(errors.Keys, k => k.Contains('^'));
    }

    [Fact]
    public async Task ReExecuteAsync_clears_route_values_and_mrx_items_only()
    {
        ServiceCollection services = new();
        services.AddExecutionPipelineRecording();
        DefaultHttpContext context = new() { RequestServices = services.BuildServiceProvider() };

        context.Request.RouteValues["id"] = "42";
        context.Items["Mrx.Some.State"] = new object();
        context.Items["Unrelated"] = "kept";

        Dictionary<string, object?>? seenRouteValues = null;
        bool? sawMrxItem = null;
        object? unrelated = null;
        context.RequestServices.GetRequiredService<IExecutionPipelineRecorder>().RecordPipeline(ctx =>
        {
            seenRouteValues = new Dictionary<string, object?>(ctx.Request.RouteValues);
            sawMrxItem = ctx.Items.ContainsKey("Mrx.Some.State");
            unrelated = ctx.Items["Unrelated"];
            return Task.CompletedTask;
        });

        await context.ReExecuteAsync();

        Assert.Empty(seenRouteValues!);
        Assert.False(sawMrxItem);
        Assert.Equal("kept", unrelated);
    }
}