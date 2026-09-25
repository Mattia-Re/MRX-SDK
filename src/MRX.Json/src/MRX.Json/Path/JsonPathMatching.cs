using System.Text.RegularExpressions;

namespace MRX.Json.Path;

/// <summary>
/// Provides compiled regular expressions used to tokenize JSON paths.
/// </summary>
public static partial class JsonPathMatching
{
    /// <summary>
    /// Matches individual word-character keys within a JSON path.
    /// </summary>
    [GeneratedRegex(@"\w+", RegexOptions.None, 1000)]
    public static partial Regex JsonPathKeys();

    /// <summary>
    /// Matches the next token in a JSON path: a property name, a quoted string array index, or a numeric
    /// array index. Captures are exposed via the <c>property</c>, <c>str_idx</c>/<c>quote</c>, and
    /// <c>num_idx</c> named groups respectively.
    /// </summary>
    [GeneratedRegex(@"(?<property>\w+)|\[(?:(?<quote>[""'])(?<str_idx>.+)\k<quote>|(?<num_idx>\d+))\]",
        RegexOptions.None, 1000)]
    public static partial Regex JsonPathTokens();
}