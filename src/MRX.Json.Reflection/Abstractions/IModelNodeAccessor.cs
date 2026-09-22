using MRX.Json.Path;

namespace MRX.Json.Reflection.Abstractions;

public interface IModelNodeAccessor
{
    bool CanHandle(JsonPathTokenType tokenType, string token, object container);
    object? GetValue(string token, object container);
}