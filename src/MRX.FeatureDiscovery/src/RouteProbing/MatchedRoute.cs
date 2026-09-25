using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace MRX.FeatureDiscovery.RouteProbing;

/// <summary>
/// Pairs a compiled route template matcher with the endpoint it resolves to, so a candidate path can be
/// tested against the route without re-parsing its template.
/// </summary>
/// <param name="Matcher">The matcher compiled from the endpoint's route template.</param>
/// <param name="Endpoint">The endpoint that the route resolves to when matched.</param>
public record MatchedRoute(TemplateMatcher Matcher, RouteEndpoint Endpoint);