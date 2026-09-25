using System.Reflection;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;

namespace MRX.Json.Reflection.PropertyAccess;

/// <summary>
/// Resolves property-name tokens against a container by case-insensitive reflection lookup.
/// </summary>
internal class StringNodeAccessor : IModelNodeAccessor, IModelNodeNameAccessor
{
    /// <summary>
    /// Determines whether <paramref name="tokenType"/> is a property-name token.
    /// </summary>
    /// <param name="tokenType">The kind of path token being resolved.</param>
    /// <param name="token">The raw token text extracted from the JSON path.</param>
    /// <param name="container">The object instance the token should be resolved against.</param>
    /// <returns><see langword="true"/> if <paramref name="tokenType"/> is <see cref="JsonPathTokenType.Property"/>.</returns>
    public bool CanHandle(JsonPathTokenType tokenType, string token, object container)
        => tokenType == JsonPathTokenType.Property;

    /// <summary>
    /// Gets the value of the property on <paramref name="container"/> whose name matches <paramref name="token"/>
    /// case-insensitively.
    /// </summary>
    /// <param name="token">The property name to look up.</param>
    /// <param name="container">The object to read the property from.</param>
    /// <returns>The property's value, or <see langword="null"/> if no matching property is found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="token"/> or <paramref name="container"/> is <see langword="null"/>.</exception>
    public object? GetValue(string token, object container)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(container);

        return GetProperty(token, container)?.GetValue(container);
    }

    /// <summary>
    /// Gets the canonical (case-correct) name of the property on <paramref name="container"/> matching
    /// <paramref name="token"/> case-insensitively.
    /// </summary>
    /// <param name="token">The property name to look up.</param>
    /// <param name="container">The object to look up the property on.</param>
    /// <returns>The property's canonical name, or <see langword="null"/> if no matching property is found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="token"/> or <paramref name="container"/> is <see langword="null"/>.</exception>
    public string? GetNodeName(string token, object container)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(container);

        return GetProperty(token, container)?.Name;
    }

    /// <summary>
    /// Finds the single property on <paramref name="container"/>'s type whose name matches
    /// <paramref name="token"/>, ignoring case.
    /// </summary>
    /// <param name="token">The property name to look up.</param>
    /// <param name="container">The object whose type is searched.</param>
    /// <returns>The matching <see cref="PropertyInfo"/>, or <see langword="null"/> if none matches.</returns>
    private static PropertyInfo? GetProperty(string token, object container)
    {
        return container.GetType().GetProperties()
            .SingleOrDefault(p => p.Name.Equals(token, StringComparison.OrdinalIgnoreCase));
    }
}