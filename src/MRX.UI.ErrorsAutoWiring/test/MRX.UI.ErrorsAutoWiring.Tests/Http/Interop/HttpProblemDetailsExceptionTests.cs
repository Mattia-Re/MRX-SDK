using System.Text.Json;
using MRX.UI.ErrorsAutoWiring.Http.Interop;

namespace MRX.UI.ErrorsAutoWiring.Tests.Http.Interop;

public class HttpProblemDetailsExceptionTests
{
    private record TraceInfo(string TraceId, int RetryCount);

    [Fact]
    public void TryGetExtension_WhenKeyMatchesExactly_ReturnsValue()
    {
        HttpProblemDetailsException sut = Deserialize("""
                                                      {
                                                          "type": "about:blank",
                                                          "status": 400,
                                                          "traceId": "abc-123"
                                                      }
                                                      """);

        bool found = sut.TryGetExtension("traceId", out string? value);

        Assert.True(found);
        Assert.Equal("abc-123", value);
    }

    [Theory]
    [InlineData("traceId")]
    [InlineData("TRACEID")]
    [InlineData("TraceId")]
    [InlineData("traceID")]
    public void TryGetExtension_KeyLookupIsCaseInsensitive(string lookupKey)
    {
        HttpProblemDetailsException sut = Deserialize("""
                                                      {
                                                          "type": "about:blank",
                                                          "status": 400,
                                                          "traceId": "abc-123"
                                                      }
                                                      """);

        bool found = sut.TryGetExtension(lookupKey, out string? value);

        Assert.True(found);
        Assert.Equal("abc-123", value);
    }

    [Fact]
    public void TryGetExtension_ValueDeserializationIsPropertyNameCaseInsensitive()
    {
        HttpProblemDetailsException sut = Deserialize("""
                                                      {
                                                          "type": "about:blank",
                                                          "status": 400,
                                                          "trace": { "TRACEID": "abc-123", "retrycount": 2 }
                                                      }
                                                      """);

        bool found = sut.TryGetExtension("trace", out TraceInfo? value);

        Assert.True(found);
        Assert.Equal("abc-123", value!.TraceId);
        Assert.Equal(2, value.RetryCount);
    }

    [Fact]
    public void TryGetExtension_WhenKeyNotFound_ReturnsFalse()
    {
        HttpProblemDetailsException sut = Deserialize("""
                                                      {
                                                          "type": "about:blank",
                                                          "status": 400
                                                      }
                                                      """);

        bool found = sut.TryGetExtension("missing", out string? value);

        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetExtension_WhenExtensionsIsNull_ReturnsFalse()
    {
        HttpProblemDetailsException sut = new();

        bool found = sut.TryGetExtension("traceId", out string? value);

        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetExtension_WhenExtensionValueIsJsonNull_ReturnsFalse()
    {
        HttpProblemDetailsException sut = Deserialize("""
                                                      {
                                                          "type": "about:blank",
                                                          "status": 400,
                                                          "traceId": null
                                                      }
                                                      """);

        bool found = sut.TryGetExtension("traceId", out string? value);

        Assert.False(found);
        Assert.Null(value);
    }

    private static HttpProblemDetailsException Deserialize(string json)
    {
        return JsonSerializer.Deserialize<HttpProblemDetailsException>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}