namespace MRX.Parsing.Tests;

public class GenericParserTests
{
    [Fact]
    public void ParseValue_ParsesCorrectly()
    {
        // Act
        object strToInt = GenericParser.ParseValue<int>("1");
        object strToUint = GenericParser.ParseValue<uint>("1");
        object strToByte = GenericParser.ParseValue<byte>("1");

        // Assert

        Assert.IsType<int>(strToInt);
        Assert.Equal(1, strToInt);

        Assert.IsType<uint>(strToUint);
        Assert.Equal((uint)1, strToUint);

        Assert.IsType<byte>(strToByte);
        Assert.Equal((byte)1, strToByte);
    }
}