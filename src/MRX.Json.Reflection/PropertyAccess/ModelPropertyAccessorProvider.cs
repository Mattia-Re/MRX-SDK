using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;

namespace MRX.Json.Reflection.PropertyAccess;

/// <summary>
/// Selects the first registered <see cref="IModelNodeAccessor"/> that can handle a given JSON path token.
/// </summary>
/// <param name="accessors">The registered accessors, tried in order.</param>
internal class ModelPropertyAccessorProvider(IEnumerable<IModelNodeAccessor> accessors)
    : IModelPropertyAccessorProvider
{
    /// <summary>
    /// Finds the first registered accessor whose <see cref="IModelNodeAccessor.CanHandle"/>
    /// returns <see langword="true"/> for the given token.
    /// </summary>
    /// <param name="tokenType">The kind of path token being resolved.</param>
    /// <param name="token">The raw token text extracted from the JSON path.</param>
    /// <param name="container">The object instance the token should be resolved against.</param>
    /// <returns>The matching accessor, or <see langword="null"/> if none can handle it.</returns>
    public IModelNodeAccessor? GetAccessor(JsonPathTokenType tokenType, string token, object container)
    {
        return accessors.FirstOrDefault(accessor => accessor.CanHandle(tokenType, token, container));
    }
}