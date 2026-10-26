using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using MRX.Core.Routing.Abstractions;

namespace MRX.Core.Routing;

/// <summary>
///     Provides extension methods for rewriting a request internally by re-running the recorded request pipeline.
/// </summary>
public static class RoutingExtensions
{
    extension(HttpContext context)
    {
        /// <summary>
        ///     Clears the matched endpoint, the route values and any state MRX keeps in
        ///     <see cref="HttpContext.Items" />, then runs the recorded pipeline again, so that routing is performed
        ///     against the current <see cref="HttpRequest.Path" />.
        /// </summary>
        /// <returns>A task that completes when the re-executed pipeline completes.</returns>
        /// <exception cref="ReExecutionTooDeepException">The request was re-executed too many times.</exception>
        /// <exception cref="InvalidOperationException">No pipeline was recorded.</exception>
        public Task ReExecuteAsync()
        {
            IExecutionPipelineRecorder recorder =
                context.RequestServices.GetRequiredService<IExecutionPipelineRecorder>();

            // The rewritten request must not inherit anything from the one it replaces
            context.SetEndpoint(null);
            context.Features.Get<IRouteValuesFeature>()?.RouteValues.Clear();
            MrxItems.Clear(context.Items);
            return recorder.ReExecute(context);
        }
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the services required to record and re-execute the request pipeline.
        /// </summary>
        /// <returns>The same <see cref="IServiceCollection" /> so that calls can be chained.</returns>
        public IServiceCollection AddExecutionPipelineRecording()
        {
            services.AddTransient<ExecutionPipelineRecordingMiddleware>();
            services.AddScoped<IExecutionPipelineRecorder, ExecutionPipelineRecorder>();
            return services;
        }
    }

    extension(IApplicationBuilder app)
    {
        /// <summary>
        ///     Adds the middleware that records the request pipeline, followed by routing. Middleware registered before
        ///     this call is not run again when a request is re-executed.
        /// </summary>
        /// <returns>The same <see cref="IApplicationBuilder" /> so that calls can be chained.</returns>
        public IApplicationBuilder UseExecutionPipelineRecording()
        {
            app.UseMiddleware<ExecutionPipelineRecordingMiddleware>();
            app.UseRouting();
            
            return app;
        }
    }
}