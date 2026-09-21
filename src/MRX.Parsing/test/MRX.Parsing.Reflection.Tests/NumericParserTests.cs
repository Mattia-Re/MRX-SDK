using System.Globalization;

namespace MRX.Parsing.Reflection.Tests;

file sealed class NonNumericType
{
}

public class NumericParserTests
{
    [Fact]
    public void Parse_Throws_WhenNumericTypeIsNull()
    {
        // Arrange
        NumericParser parser = new();

        // Act
        Action act = () => parser.Parse(null!, "1");

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Parse_Throws_WhenTokenIsNull()
    {
        // Arrange
        NumericParser parser = new();

        // Act
        Action act = () => parser.Parse(typeof(int), null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Parse_Throws_WhenTypeDoesNotImplementINumber()
    {
        // Arrange
        NumericParser parser = new();
        Type nonNumericType = typeof(NonNumericType);

        // Act
        Action act = () => parser.Parse(nonNumericType, "1");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Parse_Throws_WhenTypeIsStringEvenThoughItIsParsable()
    {
        // Arrange
        // string implements IParsable<string> but not INumber<string> —
        // confirms the INumber<> gate rejects it independently of IParsable.
        NumericParser parser = new();

        // Act
        Action act = () => parser.Parse(typeof(string), "abc");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Parse_ReturnsParsedValue_WhenTypeImplementsINumber()
    {
        // Arrange
        NumericParser parser = new();

        // Act
        object result = parser.Parse(typeof(int), "42");

        // Assert
        Assert.Equal(42, result);
        Assert.IsType<int>(result);
    }

    [Fact]
    public void Parse_ReturnsParsedValue_ForDecimalType()
    {
        // Arrange
        NumericParser parser = new();

        // Act
        object result = parser.Parse(typeof(decimal), "99.5", CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(99.5m, result);
    }
}