using Microsoft.AspNetCore.Components.Forms;
using MRX.Core.Common.ModelBinding;

namespace MRX.UI.ErrorsAutoWiring.Abstractions;

public interface IErrorMappingFactory
{
    public Dictionary<FieldIdentifier, List<CodedError>> CreateErrorFieldMap(EditContext editContext,
        Dictionary<string, CodedError[]> errors,
        HttpResponseMessage response);
}