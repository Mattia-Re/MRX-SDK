using Microsoft.AspNetCore.Http;

namespace MRX.Core.Routing.Abstractions;

public interface IExecutionPipelineRecorder
{
    /// <summary>
    /// Sets the recorded pipeline to the value of <paramref name="pipeline"/>.
    /// </summary>
    /// <param name="pipeline"></param>
    void RecordPipeline(RequestDelegate pipeline);
    
    /// <summary>
    /// Returns whether the request has been rewritten too many times.
    /// </summary>
    bool IsTooDeep();

    /// <summary>
    /// Executes the recorded pipeline with the supplied <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The context to use in the new pipeline execution.</param>
    Task ReExecute(HttpContext context);
}