using MRX.Json.Path;

namespace MRX.Json.Reflection.Abstractions;

internal interface IModelPropertyAccessorProvider
{
    IModelNodeAccessor? GetAccessor(JsonPathTokenType tokenType, string token, object container);
}