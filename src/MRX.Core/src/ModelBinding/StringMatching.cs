using System.Text.RegularExpressions;

namespace MRX.Core.ModelBinding;

internal static partial class StringMatching
{
    [GeneratedRegex(@"\w+", RegexOptions.None, 1000)]
    internal static partial Regex JsonPathTokens();
}