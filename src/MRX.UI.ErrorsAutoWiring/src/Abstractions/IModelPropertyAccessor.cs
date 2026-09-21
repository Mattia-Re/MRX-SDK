using MRX.Json.Path;

namespace MRX.UI.ErrorsAutoWiring.Abstractions;

internal interface IModelPropertyAccessor
{
    bool CanHandle(JsonPathTokenType tokenType, string token, object container);
    object? GetValue(string token, object container);
}