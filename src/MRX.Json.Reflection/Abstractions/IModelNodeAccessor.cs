using MRX.Json.Path;

namespace MRX.Json.Reflection.Abstractions;

/// <summary>
/// Resolves a single JSON path token against a model instance, reading the value it refers to.
/// </summary>
public interface IModelNodeAccessor
{
    /// <summary>
    /// Determines whether this accessor is able to resolve <paramref name="token"/> against <paramref name="container"/>.
    /// </summary>
    /// <param name="tokenType">The kind of path token being resolved (property, array index, etc.).</param>
    /// <param name="token">The raw token text extracted from the JSON path.</param>
    /// <param name="container">The object instance the token should be resolved against.</param>
    /// <returns><see langword="true"/> if this accessor can handle the token; otherwise, <see langword="false"/>.</returns>
    bool CanHandle(JsonPathTokenType tokenType, string token, object container);

    /// <summary>
    /// Resolves the value referred to by <paramref name="token"/> on <paramref name="container"/>.
    /// </summary>
    /// <param name="token">The raw token text extracted from the JSON path.</param>
    /// <param name="container">The object instance to read the value from.</param>
    /// <returns>The resolved value, or <see langword="null"/> if the token does not refer to a value.</returns>
    object? GetValue(string token, object container);
}
