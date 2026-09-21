using System.Globalization;
using System.Reflection;

namespace MRX.Parsing.Reflection.Tests;

public class RuntimeGenericParserTests
{
    [Fact]
    public void Parse_ReturnsParsedInt_WhenTargetTypeIsInt()
    {
        // Arrange
        Type targetType = typeof(int);
        string value = "42";

        // Act
        object result = RuntimeGenericParser.Parse(targetType, value);

        // Assert
        Assert.Equal(42, result);
        Assert.IsType<int>(result);
    }

    [Fact]
    public void Parse_ReturnsParsedDouble_WhenTargetTypeIsDouble()
    {
        // Arrange
        Type targetType = typeof(double);
        const string value = "3.14";

        // Act
        object result = RuntimeGenericParser.Parse(targetType, value, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(3.14, result);
        Assert.IsType<double>(result);
    }

    [Fact]
    public void Parse_ReturnsParsedDecimal_WhenTargetTypeIsDecimal()
    {
        // Arrange
        Type targetType = typeof(decimal);
        const string value = "123.45";

        // Act
        object result = RuntimeGenericParser.Parse(targetType, value, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(123.45m, result);
        Assert.IsType<decimal>(result);
    }

    [Fact]
    public void Parse_ReturnsParsedLong_WhenTargetTypeIsLong()
    {
        // Arrange
        Type targetType = typeof(long);
        const string value = "9999999999";

        // Act
        object result = RuntimeGenericParser.Parse(targetType, value);

        // Assert
        Assert.Equal(9999999999L, result);
        Assert.IsType<long>(result);
    }

    [Fact]
    public void Parse_UsesProvidedFormatProvider_ForCultureSpecificParsing()
    {
        // Arrange
        Type targetType = typeof(double);
        const string value = "3,14"; // comma decimal separator
        CultureInfo germanCulture = CultureInfo.GetCultureInfo("de-DE");

        // Act
        object result = RuntimeGenericParser.Parse(targetType, value, germanCulture);

        // Assert
        Assert.Equal(3.14, result);
    }

    [Fact]
    public void Parse_ResultsAreCached_AcrossRepeatedCallsWithSameTargetType()
    {
        // Arrange
        Type targetType = typeof(int);

        // Act
        object first = RuntimeGenericParser.Parse(targetType, "1");
        object second = RuntimeGenericParser.Parse(targetType, "2");

        // Assert
        // Not directly observable that the MethodInfo was cached (private field),
        // but repeated calls for the same type must each still parse correctly,
        // proving the cached MethodInfo remains usable across calls.
        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }

    [Fact]
    public void Parse_Throws_WhenValueIsNotParsableAsTargetType()
    {
        // Arrange
        Type targetType = typeof(int);
        const string value = "not-a-number";

        // Act
        Action act = () => RuntimeGenericParser.Parse(targetType, value);

        // Assert
        // int.Parse throws FormatException; reflection invocation via MethodInfo.Invoke
        // wraps it in TargetInvocationException, with the original as InnerException.
        TargetInvocationException exception = Assert.Throws<TargetInvocationException>(act);
        Assert.IsType<FormatException>(exception.InnerException);
    }

    [Fact]
    public void Parse_Throws_WhenTargetTypeDoesNotImplementIParsable()
    {
        // Arrange
        Type targetType = typeof(object); // object does not implement IParsable<object>
        const string value = "anything";

        // Act
        Action act = () => RuntimeGenericParser.Parse(targetType, value);

        // Assert
        // MakeGenericMethod fails to satisfy the `where T : IParsable<T>` constraint.
        Assert.Throws<ArgumentException>(act);
    }
}