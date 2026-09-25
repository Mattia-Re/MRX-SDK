using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using MRX.Core.Features;
using MRX.FeatureDiscovery.Common;
using MRX.FeatureDiscovery.RouteProbing;

namespace MRX.FeatureDiscovery.Controllers;

/// <summary>
/// Exposes an endpoint that reports which MRX features are enabled for a given request path and method.
/// </summary>
/// <param name="routeProbe">Used to resolve the route endpoint matching a probed path and method.</param>
[ApiController]
public class FeatureDiscoveryController(RouteProbe routeProbe) : ControllerBase
{
    /// <summary>
    /// Resolves the route matching <paramref name="path"/> and <paramref name="method"/> and returns the
    /// MRX features enabled for it.
    /// </summary>
    /// <param name="path">The request path of the endpoint being probed.</param>
    /// <param name="method">An HTTP method supported by the endpoint being probed.</param>
    /// <returns>
    /// A 200 OK response containing the <see cref="FeatureDiscoveryResponse"/> for the matched endpoint,
    /// or a 404 Not Found response if no endpoint matches.
    /// </returns>
    [HttpOptions("{*path}")]
    [EndpointSummary("Discover endpoint features")]
    [EndpointDescription("Returns MRX features enabled for the endpoint hosted at the request path.")]
    [ProducesResponseType<FeatureDiscoveryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public IActionResult DiscoverEndpointFeatures(
        string path,
        
        [FromHeader(Name = "X-Probe-Method")]
        [Description("An HTTP method supported by the endpoint being probed.")] string method)
    {
        RouteEndpoint? endpoint = routeProbe.GetGenericRouteEndpoint($"/{path}", method);
        if (endpoint == null)
            return NotFound();

        IReadOnlyCollection<IFeatureDescriptor> features =
            endpoint.Metadata.GetOrderedMetadata<IFeatureDescriptor>();

        return Ok(new FeatureDiscoveryResponse(features.Select(f => f.FeatureName)));
    }
}