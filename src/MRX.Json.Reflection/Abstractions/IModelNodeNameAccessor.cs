namespace MRX.Json.Reflection.Abstractions;

/// <summary>
/// Resolves the canonical member name that a JSON path token maps to on a model instance.
/// </summary>
public interface IModelNodeNameAccessor
{
    /// <summary>
    /// Gets the canonical name of the member that <paramref name="token"/> refers to on <paramref name="container"/>.
    /// </summary>
    /// <param name="token">The raw token text extracted from the JSON path.</param>
    /// <param name="container">The object instance the token should be resolved against.</param>
    /// <returns>The canonical member name, or <see langword="null"/> if it cannot be resolved.</returns>
    string? GetNodeName(string token, object container);
}
