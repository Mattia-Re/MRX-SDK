using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MRX.Core.Routing.Abstractions;

namespace MRX.Core.Routing;

/// <summary>
///     Middleware that records the rest of the request pipeline in the request's
///     <see cref="IExecutionPipelineRecorder" />, so that it can later be executed again by
///     <see cref="RoutingExtensions.ReExecuteAsync" />.
/// </summary>
public class ExecutionPipelineRecordingMiddleware : IMiddleware
{
    /// <inheritdoc />
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.RequestServices.GetRequiredService<IExecutionPipelineRecorder>().RecordPipeline(next);
        return next(context);
    }
}