# Internal Rewrites

## What it is

Sometimes the code handling a request needs to hand it to a different endpoint without involving the
client: rendering a view from a context that can't return it directly, or routing to an error action
that isn't reachable through any public route. **Execution pipeline recording** supports this by letting
an action rewrite the request path and have the app route the request again, as if it had arrived at the
new path.

## Setting it up

```csharp
builder.Services.AddExecutionPipelineRecording();
// ...
app.UseExecutionPipelineRecording();
app.MapControllers();
```

[`UseExecutionPipelineRecording`](xref:MRX.Core.Routing.RoutingExtensions) adds a middleware that
records everything downstream of it, then calls `UseRouting()` itself. Middleware registered *before*
it, such as HTTPS redirection, runs once per request and is not run again on a rewrite.

## Rewriting a request

Set the new path and return
[`ReExecuteResult`](xref:MRX.Core.Routing.ReExecuteResult):

```csharp
[Route("/orders/archived")]
public IActionResult Archived()
{
    HttpContext.Request.Path = "/errors/gone";
    return new ReExecuteResult();
}
```

`HttpContext.ReExecuteAsync()` does the same thing from code that isn't an action. It clears the matched
endpoint and runs the recorded pipeline again, so routing matches the current `Request.Path`.

## Not a new request

A rewrite reuses the same `HttpContext`, so it is not a new request from the client and carries no new
client data. Rewrite targets should therefore not rely on model binding of client-supplied values. If the
client must make a new request, use a real redirect instead.

## Loop protection

A request can be re-executed at most 100 times. Beyond that,
[`ReExecutionTooDeepException`](xref:MRX.Core.Routing.ReExecutionTooDeepException) is thrown, which usually
means two rewrites are sending the request back and forth. Unless handled, it results in a 500 response.
