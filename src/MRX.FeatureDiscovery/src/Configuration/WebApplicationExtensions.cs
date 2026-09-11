using Microsoft.AspNetCore.Builder;

namespace MRX.FeatureDiscovery.Configuration;

public static class WebApplicationExtensions
{
    extension(WebApplication app)   
    {
        public WebApplication UseMrxFeatureDiscovery()
        {
            // Map controllers so that the controller handling route discovery is loaded
            app.MapControllers();
            return app;
        }
    }
}