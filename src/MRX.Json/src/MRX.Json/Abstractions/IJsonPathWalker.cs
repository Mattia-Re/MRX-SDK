using MRX.Json.Path;

namespace MRX.Json.Abstractions;

internal interface IJsonPathWalker
{
    bool MoveNext(out JsonPathToken token);
}