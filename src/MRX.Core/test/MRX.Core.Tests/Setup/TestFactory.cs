using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MRX.Core.Tests.Setup;

public class TestFactory : WebApplicationFactory<Program>
{
    public EndpointCapture Capture { get; } = new();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(Capture);
            services.AddSingleton<IStartupFilter, EndpointCaptureFilter>();
        });
    }
}