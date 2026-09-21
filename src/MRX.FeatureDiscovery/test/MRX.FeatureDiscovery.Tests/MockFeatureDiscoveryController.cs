using Microsoft.AspNetCore.Mvc;
using MRX.Core.ModelBinding.Attributes;

namespace MRX.FeatureDiscovery.Tests;

[ApiController]
public class MockFeatureDiscoveryController : ControllerBase
{
    [HttpPost("/disambiguate")]
    [Disambiguate]
    public IActionResult Disambiguate() => Ok();

    [HttpPost("/no-features")]
    public IActionResult NoFeatures() => Ok();
}