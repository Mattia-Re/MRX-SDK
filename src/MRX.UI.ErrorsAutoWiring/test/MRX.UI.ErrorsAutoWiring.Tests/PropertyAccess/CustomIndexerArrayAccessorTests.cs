using Moq;
using MRX.Json.Path;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.PropertyAccess;

namespace MRX.UI.ErrorsAutoWiring.Tests.PropertyAccess;

file sealed class NumericIndexerContainer(Dictionary<int, string> values)
{
    public string this[int index] =>
        values.TryGetValue(index, out string? value) ? value : throw new IndexOutOfRangeException();
}

file sealed class NoIndexerContainer
{
}

public class CustomIndexerArrayAccessorTests
{
    [Fact]
    public void Constructor_Throws_WhenNumericParserIsNull()
    {
        // Act
        Action act = () => new CustomIndexerArrayAccessor(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void CanHandle_ReturnsTrue_WhenTokenTypeIsArrayIndex()
    {
        // Arrange
        Mock<INumericTokenParser> numericParserMock = new();
        CustomIndexerArrayAccessor accessor = new(numericParserMock.Object);
        NumericIndexerContainer container = new(new Dictionary<int, string>());

        // Act
        bool result = accessor.CanHandle(JsonPathTokenType.ArrayIndex, "0", container);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_ReturnsFalse_WhenTokenTypeIsNotArrayIndex()
    {
        // Arrange
        Mock<INumericTokenParser> numericParserMock = new();
        CustomIndexerArrayAccessor accessor = new(numericParserMock.Object);
        NumericIndexerContainer container = new(new Dictionary<int, string>());

        // Act
        bool result = accessor.CanHandle(JsonPathTokenType.Property, "0", container);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetValue_ReturnsIndexerValue_WhenIndexerExists()
    {
        // Arrange
        Mock<INumericTokenParser> numericParserMock = new();
        numericParserMock
            .Setup(parser => parser.Parse(typeof(int), "1"))
            .Returns(1);
        CustomIndexerArrayAccessor accessor = new(numericParserMock.Object);
        NumericIndexerContainer container = new(new Dictionary<int, string> { [1] = "found" });

        // Act
        object? result = accessor.GetValue("1", container);

        // Assert
        Assert.Equal("found", result);
        numericParserMock.Verify(parser => parser.Parse(typeof(int), "1"), Times.Once);
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenIndexerThrowsIndexOutOfRangeException()
    {
        // Arrange
        Mock<INumericTokenParser> numericParserMock = new();
        numericParserMock
            .Setup(parser => parser.Parse(typeof(int), "99"))
            .Returns(99);
        CustomIndexerArrayAccessor accessor = new(numericParserMock.Object);
        NumericIndexerContainer container = new(new Dictionary<int, string>());

        // Act
        object? result = accessor.GetValue("99", container);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_Throws_WhenContainerHasNoNumericIndexer()
    {
        // Arrange
        Mock<INumericTokenParser> numericParserMock = new();
        CustomIndexerArrayAccessor accessor = new(numericParserMock.Object);
        NoIndexerContainer container = new();

        // Act
        Action act = () => accessor.GetValue("0", container);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }
}