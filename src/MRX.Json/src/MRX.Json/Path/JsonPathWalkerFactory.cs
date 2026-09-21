using MRX.Json.Abstractions;

namespace MRX.Json.Path;

internal class JsonPathWalkerFactory : IJsonPathWalkerFactory
{
    public IJsonPathWalker Create(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return new JsonPathWalker(key);
    }
}