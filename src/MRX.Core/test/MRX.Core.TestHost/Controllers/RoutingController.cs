using Microsoft.AspNetCore.Mvc;
using MRX.Core.Routing;

namespace MRX.Core.TestHost.Controllers;

[ApiController]
public class RoutingController : ControllerBase
{
    [Route("/internal-reexecute")]
    public IActionResult InternalReExecute()
    {
        HttpContext.Request.Path = "/reexecute";
        return new ReExecuteResult();
    }

    [HttpGet("/reexecute")]
    [HttpPost("/reexecute")]
    public IActionResult ReExecute() => Ok();

    [Route("/recursive-internal-reexecute")]
    public IActionResult RecursiveInternalReExecute()
    {
        HttpContext.Request.Path = "/recursive-reexecute";
        return new ReExecuteResult();
    }

    [Route("/recursive-reexecute")]
    public IActionResult RecursiveReExecute()
    {
        HttpContext.Request.Path = "/recursive-internal-reexecute";
        return new ReExecuteResult();
    }

    [HttpPost("/validated-reexecute")]
    public IActionResult ValidatedReExecute([FromQuery] int page)
    {
        HttpContext.Request.Path = "/no-disambiguate";
        return new ReExecuteResult();
    }
}