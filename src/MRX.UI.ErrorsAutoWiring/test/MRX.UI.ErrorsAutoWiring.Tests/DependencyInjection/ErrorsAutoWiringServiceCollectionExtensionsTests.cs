using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.DependencyInjection;
using MRX.UI.ErrorsAutoWiring.Http;

namespace MRX.UI.ErrorsAutoWiring.Tests.DependencyInjection;

public class ErrorsAutoWiringServiceCollectionExtensionsTests
{
    [Fact]
    public void AddErrorsAutoWiringServices_ResolvesIModelKeyPathVisitor()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddErrorsAutoWiringServices();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        // Act
        IModelKeyPathVisitor visitor = provider.GetRequiredService<IModelKeyPathVisitor>();

        // Assert
        Assert.NotNull(visitor);
    }

    [Fact]
    public void AddErrorsAutoWiringServices_ResolvesIErrorMappingFactory()
    {
        // Arrange
        // IErrorMappingFactory transitively needs IModelKeyPathVisitor, which itself needs the
        // MRX.Json.Reflection pipeline (IJsonPathWalkerFactory, IModelPropertyAccessorProvider and the
        // IModelNodeAccessor implementations) — resolving it end to end exercises that whole chain.
        ServiceCollection services = new();
        services.AddErrorsAutoWiringServices();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        // Act
        IErrorMappingFactory factory = provider.GetRequiredService<IErrorMappingFactory>();

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddErrorsAutoWiringServices_ConfiguresProblemDetailsHttpClientOptions_WithNonNullErrorMappingFactory()
    {
        // Arrange
        // ProblemDetailsHttpClientOptionsSetup is registered as an IConfigureOptions<T> and requires
        // IErrorMappingFactory to run; if that dependency can't be resolved, evaluating IOptions<T>.Value
        // throws at this point rather than at options-configuration time.
        ServiceCollection services = new();
        services.AddErrorsAutoWiringServices();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        // Act
        ProblemDetailsHttpClientOptions options = provider
            .GetRequiredService<IOptions<ProblemDetailsHttpClientOptions>>().Value;

        // Assert
        Assert.NotNull(options.ErrorMappingFactory);
    }

    [Fact]
    public void AddErrorsAutoWiringServices_ResolvesProblemDetailsHttpClient()
    {
        // Arrange
        // This is the exact type that failed to resolve before the DI registrations were added:
        // resolving it forces the typed-client factory to build ProblemDetailsHttpClientOptions, which
        // in turn resolves the full IErrorMappingFactory -> IModelKeyPathVisitor -> MRX.Json.Reflection chain.
        ServiceCollection services = new();
        services.AddErrorsAutoWiringServices();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();

        // Act
        ProblemDetailsHttpClient client = scope.ServiceProvider.GetRequiredService<ProblemDetailsHttpClient>();

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void AddErrorsAutoWiringServices_WithConfigureClientAction_AppliesConfigurationToUnderlyingHttpClient()
    {
        // Arrange
        Uri baseAddress = new("https://example.test/");
        ServiceCollection services = new();
        services.AddErrorsAutoWiringServices(client => client.BaseAddress = baseAddress);
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();

        // Act
        // ProblemDetailsHttpClient does not expose its inner HttpClient, so we resolve the same typed
        // client factory path indirectly through IHttpClientFactory, using the typed client's registered name.
        HttpClient httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(nameof(ProblemDetailsHttpClient));

        // Assert
        Assert.Equal(baseAddress, httpClient.BaseAddress);
    }

    [Fact]
    public void AddErrorsAutoWiringServices_CalledTwiceOnSameCollection_StillResolvesASingleErrorMappingFactory()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddErrorsAutoWiringServices();
        services.AddErrorsAutoWiringServices();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        // Act
        IErrorMappingFactory factory = provider.GetRequiredService<IErrorMappingFactory>();

        // Assert
        Assert.NotNull(factory);
    }
}