using Microsoft.AspNetCore.Components.Forms;
using MRX.Core.ModelBinding;

namespace MRX.UI.ErrorsAutoWiring.Http;

public class ProblemDetailsHttpClientOptions
{
    public delegate Dictionary<FieldIdentifier, List<CodedError>> ErrorMappingFactoryHandler(EditContext editContext,
        Dictionary<string, CodedError[]> errors,
        HttpResponseMessage response);

    public ErrorMappingFactoryHandler ErrorMappingFactory
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    }
}