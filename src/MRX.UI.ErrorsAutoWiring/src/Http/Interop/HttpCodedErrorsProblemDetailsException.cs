using MRX.Core.Common.ModelBinding;

namespace MRX.UI.ErrorsAutoWiring.Http.Interop;

public class HttpCodedErrorsProblemDetailsException : HttpProblemDetailsException
{
    public Dictionary<string, CodedError[]> Errors { get; set; } = [];
}