using Microsoft.AspNetCore.Components.Forms;
using MRX.Core.ModelBinding;

namespace MRX.UI.ErrorsAutoWiring.Http;

public class ProblemDetailsHttpClientOptions
{
    public delegate void ErrorFactoryHandler(EditContext editContext, string key, CodedError[] errors);

    public ErrorFactoryHandler ErrorFactory
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    }
}