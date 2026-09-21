using System.Text.RegularExpressions;
using MRX.Json.Path;

namespace MRX.Json.Tests.Path;

public class JsonPathMatchingTests
{
    [Fact]
    public void JsonPathTokens_MatchesAllTokensGrouped()
    {
        // Arrange
        const string subjStr = "obj.data[\"foo.bar\"].result[1]['baz'].text";

        // Act

        MatchCollection matches = JsonPathMatching.JsonPathTokens().Matches(subjStr);
        ILookup<string, string> groups = matches
            .SelectMany(m => m.Groups.Cast<Group>())
            .Where(g => g.Success && !char.IsDigit(g.Name[0]))
            .ToLookup(g => g.Name, g => g.Value);

        // Assert
        Assert.Equal(4, groups["property"].Count());
        Assert.Equal(2, groups["str_idx"].Count());
        Assert.Single(groups["num_idx"]);
    }
}