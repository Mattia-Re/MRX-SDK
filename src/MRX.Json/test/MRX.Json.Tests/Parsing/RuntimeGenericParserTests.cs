using MRX.Json.Parsing;

namespace MRX.Json.Tests.Parsing;

public class RuntimeGenericParserTests
{
    [Fact]
    public void RuntimeGenericParser_ParsesCorrectly()
    {
        // Act
        object strToInt = RuntimeGenericParser.Parse(typeof(int), "1");
        object strToUint = RuntimeGenericParser.Parse(typeof(uint), "1");
        object strToByte = RuntimeGenericParser.Parse(typeof(byte), "1");

        // Assert

        Assert.IsType<int>(strToInt);
        Assert.Equal(1, strToInt);

        Assert.IsType<uint>(strToUint);
        Assert.Equal((uint)1, strToUint);

        Assert.IsType<byte>(strToByte);
        Assert.Equal((byte)1, strToByte);
    }
}