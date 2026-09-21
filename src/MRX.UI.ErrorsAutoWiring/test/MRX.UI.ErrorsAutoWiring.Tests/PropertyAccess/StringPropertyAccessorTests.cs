using MRX.Json.Path;
using MRX.UI.ErrorsAutoWiring.PropertyAccess;

namespace MRX.UI.ErrorsAutoWiring.Tests.PropertyAccess;

file sealed class SamplePerson
{
    public string Name { get; set; } = string.Empty;
}

public class StringPropertyAccessorTests
{
    [Fact]
    public void CanHandle_ReturnsTrue_WhenTokenTypeIsProperty()
    {
        // Arrange
        StringPropertyAccessor accessor = new();
        SamplePerson container = new();

        // Act
        bool result = accessor.CanHandle(JsonPathTokenType.Property, "Name", container);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_ReturnsFalse_WhenTokenTypeIsNotProperty()
    {
        // Arrange
        StringPropertyAccessor accessor = new();
        SamplePerson container = new();

        // Act
        bool result = accessor.CanHandle(JsonPathTokenType.ArrayIndex, "Name", container);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetValue_ReturnsPropertyValue_UsingCaseInsensitiveMatch()
    {
        // Arrange
        StringPropertyAccessor accessor = new();
        SamplePerson container = new() { Name = "John" };

        // Act
        object? result = accessor.GetValue("name", container);

        // Assert
        Assert.Equal("John", result);
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenPropertyNotFound()
    {
        // Arrange
        StringPropertyAccessor accessor = new();
        SamplePerson container = new() { Name = "John" };

        // Act
        object? result = accessor.GetValue("NonExistentProperty", container);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_Throws_WhenTokenIsNull()
    {
        // Arrange
        StringPropertyAccessor accessor = new();
        SamplePerson container = new();

        // Act
        Action act = () => accessor.GetValue(null!, container);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GetValue_Throws_WhenContainerIsNull()
    {
        // Arrange
        StringPropertyAccessor accessor = new();

        // Act
        Action act = () => accessor.GetValue("Name", null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}