using MRX.Json.Path;
using MRX.UI.ErrorsAutoWiring.Abstractions;

namespace MRX.UI.ErrorsAutoWiring.PropertyAccess;

internal class ModelPropertyAccessorProvider(IEnumerable<IModelPropertyAccessor> accessors)
    : IModelPropertyAccessorProvider
{
    public IModelPropertyAccessor? GetAccessor(JsonPathTokenType tokenType, string token, object container) =>
        accessors.FirstOrDefault(accessor => accessor.CanHandle(tokenType, token, container));
}