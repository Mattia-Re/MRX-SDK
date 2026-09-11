using Microsoft.Extensions.DependencyInjection;
using MRX.Core.ModelBinding;

namespace MRX.Core;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMrxModelBindingValidation()
            => services.AddMrxModelBindingValidation(_ => { });

        public IServiceCollection AddMrxModelBindingValidation(Action<MrxConfigurationOptions> configureOptions)
        {
            MrxConfigurationOptions options = new();
            configureOptions(options);
            
            // Setup options
            services.Configure(configureOptions);
            
            // Configure model binding
            services.ConfigureMrxModelBindingValidation(options.ModelBindingOptions);
            
            return services;
        }
    }
}