using MRX.Json.Path;

namespace MRX.Json.Abstractions;

/// <summary>
/// Iterates a JSON path one token at a time.
/// </summary>
internal interface IJsonPathWalker
{
    /// <summary>
    /// Advances to the next token in the path.
    /// </summary>
    /// <param name="token">When this method returns <see langword="true"/>, the next token in the path.</param>
    /// <returns><see langword="true"/> if a token was found; <see langword="false"/> if the path is fully consumed.</returns>
    bool MoveNext(out JsonPathToken token);
}
