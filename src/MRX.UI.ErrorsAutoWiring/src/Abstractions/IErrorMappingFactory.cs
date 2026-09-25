using Microsoft.AspNetCore.Components.Forms;
using MRX.Core.Common.ModelBinding;

namespace MRX.UI.ErrorsAutoWiring.Abstractions;

/// <summary>
/// Maps coded errors returned by an HTTP response to the fields of the model bound to an <see cref="EditContext"/>.
/// </summary>
public interface IErrorMappingFactory
{
    /// <summary>
    /// Builds a map of field identifiers to their associated coded errors.
    /// </summary>
    /// <param name="editContext">The edit context whose model the errors should be mapped against.</param>
    /// <param name="errors">The coded errors, keyed by the server-provided error key.</param>
    /// <param name="response">The HTTP response the errors were extracted from.</param>
    /// <returns>A dictionary linking each resolved field identifier to the list of errors that apply to it.</returns>
    public Dictionary<FieldIdentifier, List<CodedError>> CreateErrorFieldMap(EditContext editContext,
        Dictionary<string, CodedError[]> errors,
        HttpResponseMessage response);
}