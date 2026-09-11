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

[ApiController]
public class FeatureDiscoveryController(RouteProbe routeProbe) : ControllerBase
{
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