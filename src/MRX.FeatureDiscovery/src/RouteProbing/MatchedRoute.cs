using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace MRX.FeatureDiscovery.RouteProbing;

public record MatchedRoute(TemplateMatcher Matcher, RouteEndpoint Endpoint);