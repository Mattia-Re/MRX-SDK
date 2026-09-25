using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MRX.Json.Reflection.DependencyInjection;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.Http;
using MRX.UI.ErrorsAutoWiring.ModelDiscovery;
using MRX.UI.ErrorsAutoWiring.Wiring;

namespace MRX.UI.ErrorsAutoWiring.DependencyInjection;

/// <summary>
/// Provides extension methods for registering the errors auto-wiring services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ErrorsAutoWiringServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the errors auto-wiring services, using an unconfigured <see cref="HttpClient"/>.
        /// </summary>
        public void AddErrorsAutoWiringServices()
            => services.AddErrorsAutoWiringServices(_ => { });

        /// <summary>
        /// Registers the errors auto-wiring services.
        /// </summary>
        /// <param name="configureClient">A delegate used to configure the underlying <see cref="HttpClient"/>.</param>
        public void AddErrorsAutoWiringServices(Action<HttpClient> configureClient)
        {
            services.AddMrxJsonReflection();

            services.TryAddTransient<IModelKeyPathVisitor, ModelKeyPathVisitor>();
            services.TryAddTransient<IErrorMappingFactory, DefaultErrorMappingFactory>();

            services.TryAddEnumerable(
                ServiceDescriptor
                    .Transient<IConfigureOptions<ProblemDetailsHttpClientOptions>,
                        ProblemDetailsHttpClientOptionsSetup>());

            services.AddHttpClient<ProblemDetailsHttpClient>(configureClient);
        }
    }
}