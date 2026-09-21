using MRX.Json.Path;

namespace MRX.UI.ErrorsAutoWiring.Abstractions;

internal interface IModelPropertyAccessorProvider
{
    IModelPropertyAccessor? GetAccessor(JsonPathTokenType tokenType, string token, object container);
}