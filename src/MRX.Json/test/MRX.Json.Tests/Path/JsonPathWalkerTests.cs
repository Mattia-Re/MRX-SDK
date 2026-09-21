using MRX.Json.Path;

namespace MRX.Json.Tests.Path;

public class JsonPathWalkerTests
{
    [Fact]
    public void Walker_ReturnsTokens_ToRebuildPath()
    {
        // Arrange
        const string subjStr = "obj.data[\"foo.bar\"].result[1]['baz'].text";
        const string expectedRebuiltStr = "obj.data[\"foo.bar\"].result[1][\"baz\"].text";
        JsonPathWalker walker = new(subjStr);

        // Act
        string rebuiltPath = string.Empty;

        while (walker.MoveNext(out JsonPathToken token))
        {
            string rebuilt = token.Type switch
            {
                JsonPathTokenType.Property => $".{token.Token}",
                JsonPathTokenType.ArrayIndex => $"[{token.Token}]",
                JsonPathTokenType.ArrayStringIndex => $"[\"{token.Token}\"]",
                _ => throw new InvalidOperationException("Unexpected JSON token type")
            };
            rebuiltPath += rebuilt;
        }

        rebuiltPath = rebuiltPath.TrimStart('.');

        // Assert
        Assert.Equal(expectedRebuiltStr, rebuiltPath);
    }
}