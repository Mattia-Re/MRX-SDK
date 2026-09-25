using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;

namespace MRX.FeatureDiscovery.RouteProbing;

/// <summary>
/// Service for discovering routes by path and method.
/// </summary>
public class RouteProbe(EndpointDataSource endpointDataSource)
{
    /// <summary>
    /// Lazily-initialized cache of all discovered application routes, populated on first access by
    /// <see cref="LazyLoadRoutes"/>.
    /// </summary>
    public Lazy<List<MatchedRoute>> MatchedRoutesCache { get; } = new(() => []);

    /// <summary>
    /// Discovers all application routes and caches them.
    /// </summary>
    /// <returns>A list of all application routes</returns>
    private List<MatchedRoute> LazyLoadRoutes()
    {
        List<MatchedRoute> routes = MatchedRoutesCache.Value;
        
        // Check if routes were already discovered and cached.
        if (routes.Count != 0)
            return routes;
        
        lock (MatchedRoutesCache)
        {
            // Read the value again in case another thread updated the value while this one was waiting on a lock
            routes = MatchedRoutesCache.Value;
            
            // Check if another thread populated the routes cache while this one was waiting on a lock
            if (routes.Count != 0)
                return routes;
            
            // Evaluate all registered endpoints
            foreach (Endpoint endpoint in endpointDataSource.Endpoints)
            {
                if (endpoint is RouteEndpoint routeEndpoint && !string.IsNullOrWhiteSpace(routeEndpoint.RoutePattern.RawText))
                {
                    // Exclude catch-all endpoints
                    string routeTemplate = routeEndpoint.RoutePattern.RawText;
                    if (routeTemplate.StartsWith("{*")) continue;
                            
                    // Store endpoints with their matchers so that the routes list can later be iterated
                    // over to find a match for a given path and extract the respective endpoint
                    RouteTemplate template = TemplateParser.Parse(routeEndpoint.RoutePattern.RawText);
                    TemplateMatcher matcher = new(template, new RouteValueDictionary());
                    routes.Add(new MatchedRoute(matcher, routeEndpoint));
                }
            }
        }

        return routes;
    }
    
    /// <summary>
    /// Finds the endpoint whose route template matches <paramref name="path"/> and that supports
    /// <paramref name="method"/>.
    /// </summary>
    /// <param name="path">The request path to match against known routes.</param>
    /// <param name="method">The HTTP method the endpoint must support.</param>
    /// <returns>The matching <see cref="RouteEndpoint"/>, or <see langword="null"/> if none is found.</returns>
    internal RouteEndpoint? GetGenericRouteEndpoint(string path, string method)
    {
        RouteValueDictionary routeValues = new();

        foreach (MatchedRoute route in LazyLoadRoutes())
        {
            routeValues.Clear();
            if (route.Matcher.TryMatch(path, routeValues) && route.Endpoint.Metadata.GetMetadata<HttpMethodActionConstraint>()?.HttpMethods.Contains(method.ToUpperInvariant()) == true)
            {
                return route.Endpoint;
            }
        }

        return null;
    }
}