using System.Collections;
using MRX.Json.Path;
using MRX.Json.Reflection.PropertyAccess;

namespace MRX.Json.Reflection.Tests.PropertyAccess;

file sealed class PlainEnumerableContainer(object[] items) : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        return items.GetEnumerator();
    }
}

file sealed class NonEnumerableContainer
{
}

public class ArrayElementAccessorTests
{
    [Fact]
    public void CanHandle_ReturnsFalse_WhenTokenTypeIsNotArrayIndex()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        List<int> container = [1, 2, 3];

        // Act
        bool result = accessor.CanHandle(JsonPathTokenType.Property, "0", container);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanHandle_ReturnsTrue_WhenContainerIsIList()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        List<int> container = [1, 2, 3];

        // Act
        bool result = accessor.CanHandle(JsonPathTokenType.ArrayIndex, "0", container);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_ReturnsTrue_WhenContainerIsPlainEnumerable()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        PlainEnumerableContainer container = new([1, 2, 3]);

        // Act
        bool result = accessor.CanHandle(JsonPathTokenType.ArrayIndex, "0", container);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_ReturnsFalse_WhenContainerIsNeitherListNorEnumerable()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        NonEnumerableContainer container = new();

        // Act
        bool result = accessor.CanHandle(JsonPathTokenType.ArrayIndex, "0", container);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanHandle_Throws_WhenTokenIsNull()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        List<int> container = [];

        // Act
        Action act = () => accessor.CanHandle(JsonPathTokenType.ArrayIndex, null!, container);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void CanHandle_Throws_WhenContainerIsNull()
    {
        // Arrange
        ArrayElementAccessor accessor = new();

        // Act
        Action act = () => accessor.CanHandle(JsonPathTokenType.ArrayIndex, "0", null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GetValue_ReturnsElement_WhenIndexWithinListBounds()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        List<string> container = ["first", "second", "third"];

        // Act
        object? result = accessor.GetValue("1", container);

        // Assert
        Assert.Equal("second", result);
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenIndexOutOfListBounds()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        List<string> container = ["first"];

        // Act
        object? result = accessor.GetValue("5", container);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_ReturnsElement_WhenContainerIsPlainEnumerable()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        PlainEnumerableContainer container = new(["x", "y", "z"]);

        // Act
        object? result = accessor.GetValue("2", container);

        // Assert
        Assert.Equal("z", result);
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenIndexOutOfEnumerableBounds()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        PlainEnumerableContainer container = new(["x"]);

        // Act
        object? result = accessor.GetValue("9", container);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_Throws_WhenTokenIsNull()
    {
        // Arrange
        ArrayElementAccessor accessor = new();
        List<int> container = [];

        // Act
        Action act = () => accessor.GetValue(null!, container);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GetValue_Throws_WhenContainerIsNull()
    {
        // Arrange
        ArrayElementAccessor accessor = new();

        // Act
        Action act = () => accessor.GetValue("0", null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}