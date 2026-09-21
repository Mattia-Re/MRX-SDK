using System.Text.RegularExpressions;

namespace MRX.Json.Path;

public static partial class JsonPathMatching
{
    [GeneratedRegex(@"\w+", RegexOptions.None, 1000)]
    public static partial Regex JsonPathKeys();

    [GeneratedRegex(@"(?<property>\w+)|\[(?:(?<quote>[""'])(?<str_idx>.+)\k<quote>|(?<num_idx>\d+))\]",
        RegexOptions.None, 1000)]
    public static partial Regex JsonPathTokens();
}