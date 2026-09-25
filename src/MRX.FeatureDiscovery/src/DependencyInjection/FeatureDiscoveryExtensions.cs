using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MRX.FeatureDiscovery.RouteProbing;

namespace MRX.FeatureDiscovery.DependencyInjection;

/// <summary>
/// Extension methods for registering and enabling MRX feature discovery.
/// </summary>
public static class FeatureDiscoveryExtensions
{
    /// <summary>
    /// Registers the services required for MRX feature discovery, including the controller that exposes
    /// the discovery endpoint and the <see cref="RouteProbe"/> used to resolve routes.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> so calls can be chained.</returns>
    public static IServiceCollection AddMrxFeatureDiscovery(this IServiceCollection services)
    {
        services.AddControllers()
            .AddApplicationPart(Assembly.GetExecutingAssembly());
        services.TryAddTransient<RouteProbe>();

        return services;
    }

    /// <summary>
    /// Maps the controllers required to serve the MRX feature discovery endpoint.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> so calls can be chained.</returns>
    public static WebApplication UseMrxFeatureDiscovery(this WebApplication app)
    {
        // Map controllers so that the controller handling route discovery is loaded
        app.MapControllers();
        return app;
    }
}