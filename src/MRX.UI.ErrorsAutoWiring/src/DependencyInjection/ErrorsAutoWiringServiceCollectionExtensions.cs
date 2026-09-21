using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MRX.UI.ErrorsAutoWiring.Http;

namespace MRX.UI.ErrorsAutoWiring.DependencyInjection;

public static class ErrorsAutoWiringServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddErrorsAutoWiringServices()
            => services.AddErrorsAutoWiringServices(_ => { });

        public void AddErrorsAutoWiringServices(Action<HttpClient> configureClient)
        {
            services.TryAddEnumerable(
                ServiceDescriptor
                    .Transient<IConfigureOptions<ProblemDetailsHttpClientOptions>,
                        ProblemDetailsHttpClientOptionsSetup>());

            services.AddHttpClient<ProblemDetailsHttpClient>(configureClient);
        }
    }
}