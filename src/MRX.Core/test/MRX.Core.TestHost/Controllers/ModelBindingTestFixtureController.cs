using Microsoft.AspNetCore.Mvc;
using MRX.Core.ModelBinding.Attributes;

namespace MRX.Core.TestHost.Controllers;

[ApiController]
public class ModelBindingTestFixtureController : ControllerBase
{
    [HttpPost("no-disambiguate")]
    public IActionResult PostNoDisambiguate([FromBody] MyRequest body, [FromQuery] MyRequest query)
    {
        return Ok();
    }

    [HttpPost("disambiguate")]
    [Disambiguate]
    public IActionResult PostDisambiguate([FromBody] MyRequest body, [FromQuery] MyRequest query)
    {
        return Ok();
    }

    public record MyRequest(int Id, MyObject Obj);

    public record MyObject(int Num);
}