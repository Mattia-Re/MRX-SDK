using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MRX.Core.Tests.Setup;

public class EndpointCaptureFilter(EndpointCapture capture) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use(async (context, nextMiddleware) =>
        {
            await nextMiddleware();
            capture.Last = context.GetEndpoint();
        });

        next(app);
    };
}