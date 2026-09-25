using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MRX.Core.DependencyInjection;
using MRX.FeatureDiscovery.DependencyInjection;

namespace MRX.FeatureDiscovery.Tests;

public class FeatureDiscoveryTestFixture : IAsyncLifetime
{
    public WebApplication App { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddControllers()
            .AddApplicationPart(Assembly.GetExecutingAssembly());

        builder.Services.AddMrxModelBindingValidation();
        builder.Services.AddMrxFeatureDiscovery();

        App = builder.Build();

        App.MapControllers();
        App.UseMrxFeatureDiscovery();

        await App.StartAsync();
        Client = App.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        await App.DisposeAsync();
    }
}