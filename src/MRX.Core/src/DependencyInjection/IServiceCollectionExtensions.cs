using Microsoft.Extensions.DependencyInjection;
using MRX.Core.Configuration;
using MRX.Core.ModelBinding;

namespace MRX.Core.DependencyInjection;

/// <summary>
///     Provides extension methods for registering MRX services on an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers all MRX services, including model binding validation, using default options.
        /// </summary>
        /// <returns>The same <see cref="IServiceCollection" /> so that calls can be chained.</returns>
        public IServiceCollection AddMrx()
        {
            services.AddMrxModelBindingValidation();

            return services;
        }

        /// <summary>
        ///     Registers MRX model binding validation services using default options.
        /// </summary>
        /// <returns>The same <see cref="IServiceCollection" /> so that calls can be chained.</returns>
        public IServiceCollection AddMrxModelBindingValidation()
        {
            return services.AddMrxModelBindingValidation(_ => { });
        }

        /// <summary>
        ///     Registers MRX model binding validation services, configured via <paramref name="configureOptions" />.
        /// </summary>
        /// <param name="configureOptions">A delegate used to configure the <see cref="MrxConfigurationOptions" />.</param>
        /// <returns>The same <see cref="IServiceCollection" /> so that calls can be chained.</returns>
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