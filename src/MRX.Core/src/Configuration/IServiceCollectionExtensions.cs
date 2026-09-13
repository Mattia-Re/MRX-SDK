using Microsoft.Extensions.DependencyInjection;
using MRX.Core.ModelBinding;

namespace MRX.Core.Configuration;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMrx()
        {
            services.AddMrxModelBindingValidation();

            return services;
        }

        public IServiceCollection AddMrxModelBindingValidation()
        {
            return services.AddMrxModelBindingValidation(_ => { });
        }

        public IServiceCollection AddMrxModelBindingValidation(Action<MrxConfigurationOptions> configureOptions)
        {
            MrxConfigurationOptions options = new();
            configureOptions(options);

            // Add required services
            services.AddControllers();

            // Setup options
            services.Configure(configureOptions);

            // Configure model binding
            services.ConfigureMrxModelBindingValidation(options.ModelBindingOptions);

            return services;
        }
    }
}