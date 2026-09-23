using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MRX.Json.Reflection.DependencyInjection;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.Http;
using MRX.UI.ErrorsAutoWiring.ModelDiscovery;
using MRX.UI.ErrorsAutoWiring.Wiring;

namespace MRX.UI.ErrorsAutoWiring.DependencyInjection;

public static class ErrorsAutoWiringServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddErrorsAutoWiringServices()
            => services.AddErrorsAutoWiringServices(_ => { });

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