using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MRX.FeatureDiscovery.RouteProbing;

namespace MRX.FeatureDiscovery.DependencyInjection;

public static class FeatureDiscoveryExtensions
{
    public static IServiceCollection AddMrxFeatureDiscovery(this IServiceCollection services)
    {
        services.AddControllers()
            .AddApplicationPart(Assembly.GetExecutingAssembly());
        services.TryAddTransient<RouteProbe>();

        return services;
    }

    public static WebApplication UseMrxFeatureDiscovery(this WebApplication app)
    {
        // Map controllers so that the controller handling route discovery is loaded
        app.MapControllers();
        return app;
    }
}