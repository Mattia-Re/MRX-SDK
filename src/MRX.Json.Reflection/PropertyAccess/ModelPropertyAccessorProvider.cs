using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;

namespace MRX.Json.Reflection.PropertyAccess;

internal class ModelPropertyAccessorProvider(IEnumerable<IModelNodeAccessor> accessors)
    : IModelPropertyAccessorProvider
{
    public IModelNodeAccessor? GetAccessor(JsonPathTokenType tokenType, string token, object container)
    {
        return accessors.FirstOrDefault(accessor => accessor.CanHandle(tokenType, token, container));
    }
}