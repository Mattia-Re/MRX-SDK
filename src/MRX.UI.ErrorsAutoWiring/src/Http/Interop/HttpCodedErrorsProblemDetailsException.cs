using MRX.Core.Common.ModelBinding;

namespace MRX.UI.ErrorsAutoWiring.Http.Interop;

/// <summary>
/// An <see cref="HttpProblemDetailsException"/> whose problem details response included an "errors" extension
/// compatible with MRX coded errors.
/// </summary>
public class HttpCodedErrorsProblemDetailsException : HttpProblemDetailsException
{
    /// <summary>
    /// The coded errors extracted from the response's "errors" extension, keyed by the server-provided error key.
    /// </summary>
    public Dictionary<string, CodedError[]> Errors { get; set; } = [];
}