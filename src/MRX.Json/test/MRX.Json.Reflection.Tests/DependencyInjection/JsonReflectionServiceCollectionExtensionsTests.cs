using Microsoft.Extensions.DependencyInjection;
using MRX.Json.Abstractions;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;
using MRX.Json.Reflection.DependencyInjection;
using MRX.Json.Reflection.PropertyAccess;
using MRX.Parsing.Reflection.Abstractions;

namespace MRX.Json.Reflection.Tests.DependencyInjection;

public class JsonReflectionServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMrxJsonReflection_ResolvesIJsonPathWalkerFactory()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMrxJsonReflection();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        // Act
        IJsonPathWalkerFactory factory = provider.GetRequiredService<IJsonPathWalkerFactory>();

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddMrxJsonReflection_ResolvesINumericParser()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMrxJsonReflection();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        // Act
        INumericParser parser = provider.GetRequiredService<INumericParser>();

        // Assert
        Assert.NotNull(parser);
    }

    [Fact]
    public void AddMrxJsonReflection_ResolvesIModelPropertyAccessorProvider()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMrxJsonReflection();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        // Act
        IModelPropertyAccessorProvider accessorProvider = provider.GetRequiredService<IModelPropertyAccessorProvider>();

        // Assert
        Assert.NotNull(accessorProvider);
    }

    [Fact]
    public void AddMrxJsonReflection_RegistersAllThreeModelNodeAccessorImplementations()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddMrxJsonReflection();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        // Act
        List<IModelNodeAccessor> accessors = [.. provider.GetServices<IModelNodeAccessor>()];

        // Assert
        Assert.Equal(3, accessors.Count);
        Assert.Contains(accessors, a => a is StringNodeAccessor);
        Assert.Contains(accessors, a => a is ArrayElementAccessor);
        Assert.Contains(accessors, a => a is CustomIndexerArrayAccessor);
    }

    [Fact]
    public void AddMrxJsonReflection_CalledTwice_DoesNotDuplicateAccessorRegistrations()
    {
        // Arrange
        // AddMrxJsonReflection() may be called by multiple consumers (e.g. once directly and once
        // transitively through another package's own DI extension) sharing the same IServiceCollection.
        ServiceCollection services = new();
        services.AddMrxJsonReflection();
        services.AddMrxJsonReflection();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        // Act
        List<IModelNodeAccessor> accessors = [.. provider.GetServices<IModelNodeAccessor>()];

        // Assert
        Assert.Equal(3, accessors.Count);
    }

    [Fact]
    public void AddMrxJsonReflection_ResolvedAccessorProvider_ResolvesAPropertyAccessorEndToEnd()
    {
        // Arrange
        // Exercises the full container-resolved pipeline (rather than manually constructed instances,
        // as in ModelPropertyAccessorProviderTests) to confirm the registered accessors actually work
        // together once wired up purely through DI.
        ServiceCollection services = new();
        services.AddMrxJsonReflection();
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        IModelPropertyAccessorProvider accessorProvider = provider.GetRequiredService<IModelPropertyAccessorProvider>();

        // Act
        IModelNodeAccessor? accessor = accessorProvider.GetAccessor(JsonPathTokenType.Property, "Foo", new object());

        // Assert
        Assert.NotNull(accessor);
        Assert.IsType<StringNodeAccessor>(accessor);
    }
}