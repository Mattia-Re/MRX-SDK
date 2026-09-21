using System.Text.RegularExpressions;
using MRX.Json.Abstractions;

namespace MRX.Json.Path;

public class JsonPathWalker(string path) : IJsonPathWalker
{
    private string _path = path;

    public bool MoveNext(out JsonPathToken token)
    {
        if (IsPathFullyConsumed()) return NoNextToken(out token);

        // Find the first token in the path. Each significant token is captured in a group.
        // To find the token we select the first named group (name isn't a digit) that succeeded matching.
        Group? nextTokenGroup = JsonPathMatching.JsonPathTokens()
            .Match(_path)
            .Groups
            .Cast<Group>()
            .SingleOrDefault(g => g.Success && !char.IsDigit(g.Name[0]) && g.Name != "quote");

        if (nextTokenGroup == null) return NoNextToken(out token);

        // We return array accessor padding to strip JSON array accessor syntax from the path
        (JsonPathTokenType tokenType, int arrayPadding) = nextTokenGroup.Name switch
        {
            "property" => (JsonPathTokenType.Property, 0),
            "num_idx" => (JsonPathTokenType.ArrayIndex, 1),
            "str_idx" => (JsonPathTokenType.ArrayStringIndex, 2),
            _ => throw new InvalidOperationException($"Unexpected JSON token group {nextTokenGroup.Name}")
        };

        // We advance in the path by removing the token we just matched and trim any accessor
        _path = _path[(nextTokenGroup.Length + arrayPadding * 2)..].TrimStart('.');

        token = new JsonPathToken(
            nextTokenGroup.Value,
            tokenType,
            _path == string.Empty
        );
        return true;
    }

    private bool IsPathFullyConsumed()
    {
        return _path == string.Empty;
    }

    private static bool NoNextToken(out JsonPathToken token)
    {
        token = default;
        return false;
    }
}