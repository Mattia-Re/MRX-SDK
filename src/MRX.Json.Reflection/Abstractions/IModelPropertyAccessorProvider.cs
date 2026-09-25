using MRX.Json.Path;

namespace MRX.Json.Reflection.Abstractions;

/// <summary>
/// Selects the <see cref="IModelNodeAccessor"/> capable of resolving a given JSON path token.
/// </summary>
public interface IModelPropertyAccessorProvider
{
    /// <summary>
    /// Finds an accessor able to resolve <paramref name="token"/> against <paramref name="container"/>.
    /// </summary>
    /// <param name="tokenType">The kind of path token being resolved (property, array index, etc.).</param>
    /// <param name="token">The raw token text extracted from the JSON path.</param>
    /// <param name="container">The object instance the token should be resolved against.</param>
    /// <returns>The matching <see cref="IModelNodeAccessor"/>, or <see langword="null"/> if none can handle it.</returns>
    IModelNodeAccessor? GetAccessor(JsonPathTokenType tokenType, string token, object container);
}
