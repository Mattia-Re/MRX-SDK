using Microsoft.AspNetCore.Http;
using MRX.Core.Routing.Abstractions;

namespace MRX.Core.Routing;

/// <inheritdoc/>
internal class ExecutionPipelineRecorder : IExecutionPipelineRecorder
{
    private RequestDelegate? _pipeline;
    private int _depth = 0;

    private const int MaxReExecutionDepth = 100;

    public void RecordPipeline(RequestDelegate pipeline)
    {
        _pipeline = pipeline;
    }

    public bool IsTooDeep() => _depth >= MaxReExecutionDepth;

    public Task ReExecute(HttpContext context)
    {
        if (_pipeline == null) throw new InvalidOperationException("No pipeline was recorded.");
        
        if (IsTooDeep())
            throw new ReExecutionTooDeepException(
                $"Re-execution reached the maximum allowed depth of {MaxReExecutionDepth}");
        
        _depth++;
        return _pipeline(context);
    }
}