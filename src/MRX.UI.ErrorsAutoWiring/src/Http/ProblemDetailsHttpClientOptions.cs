using Microsoft.AspNetCore.Components.Forms;
using MRX.Core.Common.ModelBinding;

namespace MRX.UI.ErrorsAutoWiring.Http;

/// <summary>
/// Options controlling the behavior of <see cref="ProblemDetailsHttpClient"/>.
/// </summary>
public class ProblemDetailsHttpClientOptions
{
    /// <summary>
    /// A delegate that maps coded errors from an HTTP response onto the fields of an <see cref="EditContext"/>'s model.
    /// </summary>
    /// <param name="editContext">The edit context whose model the errors should be mapped against.</param>
    /// <param name="errors">The coded errors, keyed by the server-provided error key.</param>
    /// <param name="response">The HTTP response the errors were extracted from.</param>
    /// <returns>A dictionary linking each resolved field identifier to the list of errors that apply to it.</returns>
    public delegate Dictionary<FieldIdentifier, List<CodedError>> ErrorMappingFactoryHandler(EditContext editContext,
        Dictionary<string, CodedError[]> errors,
        HttpResponseMessage response);

    /// <summary>
    /// The factory delegate used to map coded errors onto model fields. Cannot be set to <see langword="null"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when set to <see langword="null"/>.</exception>
    public ErrorMappingFactoryHandler ErrorMappingFactory
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    }
}