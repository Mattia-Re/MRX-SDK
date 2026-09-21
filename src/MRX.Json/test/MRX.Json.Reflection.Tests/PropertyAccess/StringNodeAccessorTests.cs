using MRX.Json.Path;
using MRX.Json.Reflection.PropertyAccess;

namespace MRX.Json.Reflection.Tests.PropertyAccess;

file sealed class SamplePerson
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

file sealed class NoPropertiesContainer
{
}

public class StringNodeAccessorTests
{
    [Fact]
    public void CanHandle_ReturnsTrue_WhenTokenTypeIsProperty()
    {
        // Arrange
        StringNodeAccessor accessor = new();
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
        StringNodeAccessor accessor = new();
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
        StringNodeAccessor accessor = new();
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
        StringNodeAccessor accessor = new();
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
        StringNodeAccessor accessor = new();
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
        StringNodeAccessor accessor = new();

        // Act
        Action act = () => accessor.GetValue("Name", null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GetNodeName_ReturnsPropertyName_WhenPropertyExistsWithExactCase()
    {
        // Arrange
        StringNodeAccessor accessor = new();
        SamplePerson container = new();

        // Act
        string? result = accessor.GetNodeName("Name", container);

        // Assert
        Assert.Equal("Name", result);
    }

    [Fact]
    public void GetNodeName_ReturnsPropertyName_WhenTokenDiffersOnlyInCase()
    {
        // Arrange
        StringNodeAccessor accessor = new();
        SamplePerson container = new();

        // Act
        string? result = accessor.GetNodeName("name", container);

        // Assert
        // Confirms the returned name reflects the property's declared casing
        // ("Name"), not the casing of the input token ("name").
        Assert.Equal("Name", result);
    }

    [Fact]
    public void GetNodeName_ReturnsNull_WhenPropertyDoesNotExist()
    {
        // Arrange
        StringNodeAccessor accessor = new();
        SamplePerson container = new();

        // Act
        string? result = accessor.GetNodeName("NonExistentProperty", container);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNodeName_ReturnsNull_WhenContainerHasNoProperties()
    {
        // Arrange
        StringNodeAccessor accessor = new();
        NoPropertiesContainer container = new();

        // Act
        string? result = accessor.GetNodeName("Anything", container);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNodeName_ReturnsCorrectName_WhenContainerHasMultipleProperties()
    {
        // Arrange
        StringNodeAccessor accessor = new();
        SamplePerson container = new();

        // Act
        string? result = accessor.GetNodeName("Age", container);

        // Assert
        Assert.Equal("Age", result);
    }

    [Fact]
    public void GetNodeName_Throws_WhenTokenIsNull()
    {
        // Arrange
        StringNodeAccessor accessor = new();
        SamplePerson container = new();

        // Act
        Action act = () => accessor.GetNodeName(null!, container);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GetNodeName_Throws_WhenContainerIsNull()
    {
        // Arrange
        StringNodeAccessor accessor = new();

        // Act
        Action act = () => accessor.GetNodeName("Name", null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}