using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MRX.Json.Abstractions;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;
using MRX.Json.Reflection.PropertyAccess;
using MRX.Parsing.Reflection;
using MRX.Parsing.Reflection.Abstractions;

namespace MRX.Json.Reflection.DependencyInjection;

public static class JsonReflectionServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
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