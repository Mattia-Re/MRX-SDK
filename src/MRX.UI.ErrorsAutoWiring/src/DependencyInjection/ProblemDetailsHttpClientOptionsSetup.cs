using Microsoft.Extensions.Options;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.Http;

namespace MRX.UI.ErrorsAutoWiring.DependencyInjection;

public class ProblemDetailsHttpClientOptionsSetup(IErrorMappingFactory errorMappingFactory)
    : IConfigureOptions<ProblemDetailsHttpClientOptions>
{
    public void Configure(ProblemDetailsHttpClientOptions options)
    {
        options.ErrorMappingFactory = errorMappingFactory.CreateErrorFieldMap;
    }
}