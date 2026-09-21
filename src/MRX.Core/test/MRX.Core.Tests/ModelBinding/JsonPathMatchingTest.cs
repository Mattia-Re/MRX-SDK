using System.Text.RegularExpressions;
using MRX.Json.Path;

namespace MRX.Core.Tests.ModelBinding;

public class JsonPathMatchingTest
{
    [Fact]
    public void JsonPathTokens_ExtractsAllTokens()
    {
        // Arrange
        const string path = "foo.bar[\"worlds\"][0]";

        // Act
        MatchCollection matches = JsonPathMatching.JsonPathKeys().Matches(path);

        // Assert
        Assert.Equal(4, matches.Count);
    }
}