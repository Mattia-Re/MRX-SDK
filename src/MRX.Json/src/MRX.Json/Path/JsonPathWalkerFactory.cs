using MRX.Json.Abstractions;

namespace MRX.Json.Path;

/// <summary>
/// Default <see cref="IJsonPathWalkerFactory"/> implementation, creating <see cref="JsonPathWalker"/> instances.
/// </summary>
internal class JsonPathWalkerFactory : IJsonPathWalkerFactory
{
    /// <summary>
    /// Creates a <see cref="JsonPathWalker"/> for the given JSON path.
    /// </summary>
    /// <param name="key">The JSON path to walk.</param>
    /// <returns>A new <see cref="JsonPathWalker"/> positioned at the start of <paramref name="key"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public IJsonPathWalker Create(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return new JsonPathWalker(key);
    }
}