using MRX.Json.Path;

namespace MRX.Json.Reflection.Abstractions;

internal interface IModelNodeAccessor
{
    bool CanHandle(JsonPathTokenType tokenType, string token, object container);
    object? GetValue(string token, object container);
}