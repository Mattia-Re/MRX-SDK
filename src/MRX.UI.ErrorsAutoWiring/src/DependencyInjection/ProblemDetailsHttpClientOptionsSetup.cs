using Microsoft.Extensions.Options;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.Http;

namespace MRX.UI.ErrorsAutoWiring.DependencyInjection;

/// <summary>
/// Configures <see cref="ProblemDetailsHttpClientOptions"/> to use the registered <see cref="IErrorMappingFactory"/>.
/// </summary>
/// <param name="errorMappingFactory">The error mapping factory used to build the options' error mapping delegate.</param>
public class ProblemDetailsHttpClientOptionsSetup(IErrorMappingFactory errorMappingFactory)
    : IConfigureOptions<ProblemDetailsHttpClientOptions>
{
    /// <summary>
    /// Sets <see cref="ProblemDetailsHttpClientOptions.ErrorMappingFactory"/> to the registered
    /// <see cref="IErrorMappingFactory.CreateErrorFieldMap"/> method.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    public void Configure(ProblemDetailsHttpClientOptions options)
    {
        options.ErrorMappingFactory = errorMappingFactory.CreateErrorFieldMap;
    }
}