namespace MRX.Json.Path;

/// <summary>
/// Represents a single token extracted while walking a JSON path.
/// </summary>
/// <param name="Token">The raw text of the token.</param>
/// <param name="Type">The kind of token (property name, array index, etc.).</param>
/// <param name="EndOfPath">Whether this token is the last one in the path.</param>
public readonly record struct JsonPathToken(
    string Token,
    JsonPathTokenType Type,
    bool EndOfPath
);