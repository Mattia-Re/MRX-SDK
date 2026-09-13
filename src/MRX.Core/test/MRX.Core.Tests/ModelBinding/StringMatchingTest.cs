using System.Text.RegularExpressions;
using MRX.Core.ModelBinding;

namespace MRX.Core.Tests.ModelBinding;

public class StringMatchingTest
{
    [Fact]
    public void JsonPathTokens_ExtractsAllTokens()
    {
        // Arrange
        const string path = "foo.bar[\"worlds\"][0]";

        // Act
        MatchCollection matches = StringMatching.JsonPathTokens().Matches(path);

        // Assert
        Assert.Equal(4, matches.Count);
    }
}