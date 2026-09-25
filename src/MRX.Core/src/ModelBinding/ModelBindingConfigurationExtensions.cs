using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MRX.Core.Common.ModelBinding;
using MRX.Core.ModelBinding.Validation;

namespace MRX.Core.ModelBinding;

/// <summary>
///     Provides extension methods for configuring ADR-001 compliant model binding validation on an
///     <see cref="IServiceCollection" />.
/// </summary>
public static class ModelBindingConfigurationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Configures ADR-001 compliant model binding validation using default options.
        /// </summary>
        /// <returns>The same <see cref="IServiceCollection" /> so that calls can be chained.</returns>
        public IServiceCollection ConfigureMrxModelBindingValidation()
        {
            return services.ConfigureMrxModelBindingValidation(_ => { });
        }

        /// <summary>
        ///     Configures ADR-001 compliant model binding validation: replaces the default <see cref="IObjectModelValidator" />
        ///     with <see cref="MrxObjectModelValidator" /> and installs an <see cref="ApiBehaviorOptions.InvalidModelStateResponseFactory" />
        ///     that converts validation errors into <see cref="CodedError" /> instances.
        /// </summary>
        /// <param name="configureOptions">A delegate used to configure the <see cref="ModelBindingOptions" />.</param>
        /// <returns>The same <see cref="IServiceCollection" /> so that calls can be chained.</returns>
        public IServiceCollection ConfigureMrxModelBindingValidation(Action<ModelBindingOptions> configureOptions)
        {
            // Setup options
            services.Configure(configureOptions);

            // Add ADR-001 compliant validator
            services.RemoveAll<IObjectModelValidator>();
            services.AddSingleton<IObjectModelValidator, MrxObjectModelValidator>();

            // Setup ADR-001 compliant invalid model state factory
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    // Run the default ValidationProblemDetails constructor to get an error message for
                    // ModelErrors where only an exception is set
                    ValidationProblemDetails problemDetails = new(context.ModelState);

                    // Per ADR-001 failed requests must return well-structured error data.
                    // This factory converts all ModelError to CodedError

                    Dictionary<string, CodedError[]> errors = problemDetails.Errors
                        .ToDictionary(
                            e => e.Key,
                            e => e.Value.Select(m => new CodedError("InvalidValue", m)).ToArray());

                    return new BadRequestObjectResult(new ProblemDetails
                    {
                        Type = "/errors/invalid-request",
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Bad request",
                        Detail = "One or more validation errors occured",
                        Extensions = new Dictionary<string, object?>
                        {
                            { "errors", errors }
                        }
                    });
                };
            });

            return services;
        }
    }
}