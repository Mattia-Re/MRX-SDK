namespace MRX.Json.Abstractions;

/// <summary>
/// Creates <see cref="IJsonPathWalker"/> instances for walking JSON paths.
/// </summary>
internal interface IJsonPathWalkerFactory
{
    /// <summary>
    /// Creates a walker over the given JSON path.
    /// </summary>
    /// <param name="key">The JSON path to walk.</param>
    /// <returns>A new <see cref="IJsonPathWalker"/> positioned at the start of <paramref name="key"/>.</returns>
    IJsonPathWalker Create(string key);
}
