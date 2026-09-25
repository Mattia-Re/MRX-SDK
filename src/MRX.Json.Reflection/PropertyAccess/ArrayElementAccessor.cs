using System.Collections;
using System.Diagnostics;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;

namespace MRX.Json.Reflection.PropertyAccess;

/// <summary>
/// Resolves numeric array-index tokens against <see cref="IList"/> or <see cref="IEnumerable"/> containers.
/// </summary>
internal class ArrayElementAccessor : IModelNodeAccessor
{
    /// <summary>
    /// Determines whether <paramref name="tokenType"/> is an array index and <paramref name="container"/> is a list or enumerable.
    /// </summary>
    /// <param name="tokenType">The kind of path token being resolved.</param>
    /// <param name="token">The raw token text extracted from the JSON path.</param>
    /// <param name="container">The object instance the token should be resolved against.</param>
    /// <returns><see langword="true"/> if this accessor can handle the token; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="token"/> or <paramref name="container"/> is <see langword="null"/>.</exception>
    public bool CanHandle(JsonPathTokenType tokenType, string token, object container)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(container);

        if (tokenType != JsonPathTokenType.ArrayIndex) return false;
        return container is IList or IEnumerable;
    }

    /// <summary>
    /// Gets the element at the index parsed from <paramref name="token"/>.
    /// </summary>
    /// <param name="token">The array index, as a numeric string.</param>
    /// <param name="container">The list or enumerable to read the element from.</param>
    /// <returns>The element at the given index, or <see langword="null"/> if the index is out of range.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="token"/> or <paramref name="container"/> is <see langword="null"/>.</exception>
    public object? GetValue(string token, object container)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(container);

        if (container is IList list)
        {
            int index = int.Parse(token);
            return index > list.Count - 1 ? null : list[index];
        }

        if (container is IEnumerable enumerable)
        {
            int index = int.Parse(token);
            return enumerable.Cast<object>().ElementAtOrDefault(index);
        }

        throw new UnreachableException();
    }
}