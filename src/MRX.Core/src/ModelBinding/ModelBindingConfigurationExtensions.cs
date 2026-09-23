using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MRX.Core.Common.ModelBinding;
using MRX.Core.ModelBinding.Validation;

namespace MRX.Core.ModelBinding;

public static class ModelBindingConfigurationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureMrxModelBindingValidation()
        {
            return services.ConfigureMrxModelBindingValidation(_ => { });
        }

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