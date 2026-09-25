using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MRX.Json.Abstractions;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;
using MRX.Json.Reflection.PropertyAccess;
using MRX.Parsing.Reflection;
using MRX.Parsing.Reflection.Abstractions;

namespace MRX.Json.Reflection.DependencyInjection;

/// <summary>
/// Provides extension methods for registering MRX JSON reflection services with an <see cref="IServiceCollection"/>.
/// </summary>
public static class JsonReflectionServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the JSON path walking and model reflection services (path walker factory, numeric parser,
        /// model node accessors, and accessor provider) required to resolve JSON paths against model instances.
        /// </summary>
        /// <returns>The same <see cref="IServiceCollection"/> instance, for chaining.</returns>
        public IServiceCollection AddMrxJsonReflection()
        {
            services.TryAddTransient<IJsonPathWalkerFactory, JsonPathWalkerFactory>();
            services.TryAddSingleton<INumericParser, NumericParser>();

            services.TryAddEnumerable(
            [
                ServiceDescriptor.Transient<IModelNodeAccessor, StringNodeAccessor>(),
                ServiceDescriptor.Transient<IModelNodeAccessor, ArrayElementAccessor>(),
                ServiceDescriptor.Transient<IModelNodeAccessor, CustomIndexerArrayAccessor>()
            ]);

            services.TryAddTransient<IModelPropertyAccessorProvider, ModelPropertyAccessorProvider>();

            return services;
        }
    }
}