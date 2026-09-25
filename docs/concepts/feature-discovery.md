# Feature Discovery

## Why it exists

Some MRX behaviors are opt-in per controller or action — [Disambiguation](disambiguation.md) is one
example. A client that doesn't know whether a given endpoint has a feature enabled would have to
either hardcode that assumption per endpoint or guess from response shape. **Feature Discovery**
lets a client ask the server directly, for a specific route and HTTP method: "which MRX features are
enabled here?" — instead of guessing or hardcoding.

## Setting it up

Install the package:

```
dotnet add package MRX.FeatureDiscovery
```

Then, in `Program.cs`:

```csharp
builder.Services.AddMrxFeatureDiscovery();
// ...
app.UseMrxFeatureDiscovery();
```

[`AddMrxFeatureDiscovery`](xref:MRX.FeatureDiscovery.DependencyInjection.FeatureDiscoveryExtensions)
registers the discovery controller as an MVC application part along with the
[`RouteProbe`](xref:MRX.FeatureDiscovery.RouteProbing.RouteProbe) service it depends on;
[`UseMrxFeatureDiscovery`](xref:MRX.FeatureDiscovery.DependencyInjection.FeatureDiscoveryExtensions)
maps controllers so the discovery endpoint is actually reachable.

## Querying it

The discovery endpoint responds to `OPTIONS` requests on any path, with the probed HTTP method
passed via the `X-Probe-Method` request header:

```http
OPTIONS /orders/42 HTTP/1.1
X-Probe-Method: PUT
```

```http
HTTP/1.1 200 OK
Content-Type: application/json

{ "features": ["ERROR_KEY_DISAMBIGUATION"] }
```

If no registered route matches the probed path and method, the endpoint returns `404 Not Found`.

## How a match is resolved

[`RouteProbe`](xref:MRX.FeatureDiscovery.RouteProbing.RouteProbe) lazily builds and caches a list of
every non-catch-all route, each paired with a compiled
`TemplateMatcher` as a [`MatchedRoute`](xref:MRX.FeatureDiscovery.RouteProbing.MatchedRoute) so that
a probed path can be tested against it without re-parsing the route template on every request. To
resolve a probe, it
walks that cached list looking for the first route whose template matches the probed path and whose
`HttpMethodActionConstraint` metadata includes the probed method.

Once a matching `RouteEndpoint` is found, the controller reads every
[`IFeatureDescriptor`](xref:MRX.Core.Features.IFeatureDescriptor) attached to it as endpoint metadata
and returns their feature names in a
[`FeatureDiscoveryResponse`](xref:MRX.FeatureDiscovery.Common.FeatureDiscoveryResponse). This is the
same mechanism that surfaces `[Disambiguate]`: the attribute implements `IFeatureDescriptor`, so
simply applying it to a controller or action is enough for it to be picked up here — nothing else
needs to be registered for a feature to become discoverable this way.

## Exposing your own features

Because feature discovery works off any `IFeatureDescriptor` attached as endpoint metadata, you can
make your own opt-in behaviors discoverable the same way `[Disambiguate]` is: implement
`IFeatureDescriptor` on an attribute (or any other type usable as endpoint metadata) and apply it to
the relevant controllers or actions. No changes to `MRX.FeatureDiscovery` itself are required — the
discovery endpoint reads whatever `IFeatureDescriptor` metadata is present on the matched endpoint at
request time.
