using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MRX.Core.Routing;

/// <summary>
///     An <see cref="IActionResult" /> that re-runs the recorded request pipeline, so that routing is performed again
///     against the current <see cref="HttpRequest.Path" />.
/// </summary>
/// <remarks>
///     Intended for internal rewrites, such as handing off to a non-routed error action or rendering a view from a
///     context that cannot return it directly. The rewritten request is not a new request from the client, so its
///     target should not depend on client-supplied data being model bound. Use a real redirect when the client
///     must make a new request. Requires <see cref="RoutingExtensions.AddExecutionPipelineRecording" /> and
///     <see cref="RoutingExtensions.UseExecutionPipelineRecording" />.
/// </remarks>
public class ReExecuteResult : IActionResult
{
    /// <inheritdoc />
    public Task ExecuteResultAsync(ActionContext context)
    {
        return context.HttpContext.ReExecuteAsync();
    }
}