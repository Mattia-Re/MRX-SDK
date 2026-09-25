namespace MRX.Json.Path;

/// <summary>
/// Identifies the kind of a JSON path token.
/// </summary>
public enum JsonPathTokenType
{
    /// <summary>A named object property.</summary>
    Property = 0,

    /// <summary>A numeric array index, e.g. <c>[0]</c>.</summary>
    ArrayIndex = 1,

    /// <summary>A quoted string array index, e.g. <c>["key"]</c>.</summary>
    ArrayStringIndex = 2
}