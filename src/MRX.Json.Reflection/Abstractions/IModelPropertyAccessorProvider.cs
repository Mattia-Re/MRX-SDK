using MRX.Json.Path;

namespace MRX.Json.Reflection.Abstractions;

public interface IModelPropertyAccessorProvider
{
    IModelNodeAccessor? GetAccessor(JsonPathTokenType tokenType, string token, object container);
}