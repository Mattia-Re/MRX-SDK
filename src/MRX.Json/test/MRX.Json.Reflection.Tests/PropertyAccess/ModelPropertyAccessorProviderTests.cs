using Moq;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;
using MRX.Json.Reflection.PropertyAccess;

namespace MRX.Json.Reflection.Tests.PropertyAccess;

public class ModelPropertyAccessorProviderTests
{
    private static Mock<IModelNodeAccessor> CreateAccessorMock(bool canHandle)
    {
        Mock<IModelNodeAccessor> mock = new();
        mock.Setup(a => a.CanHandle(
                It.IsAny<JsonPathTokenType>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Returns(canHandle);

        return mock;
    }

    [Fact]
    public void GetAccessor_ReturnsFirstMatchingAccessor()
    {
        // Arrange
        Mock<IModelNodeAccessor> noMatch = CreateAccessorMock(false);
        Mock<IModelNodeAccessor> match = CreateAccessorMock(true);
        ModelPropertyAccessorProvider provider = new([noMatch.Object, match.Object]);

        // Act
        IModelNodeAccessor? result = provider.GetAccessor(JsonPathTokenType.ArrayIndex, "Foo", new object());

        // Assert
        Assert.Same(match.Object, result);
    }

    [Fact]
    public void GetAccessor_StopsAtFirstMatch_IgnoresLaterMatches()
    {
        // Arrange
        Mock<IModelNodeAccessor> firstMatch = CreateAccessorMock(true);
        Mock<IModelNodeAccessor> secondMatch = CreateAccessorMock(true);
        ModelPropertyAccessorProvider provider = new([firstMatch.Object, secondMatch.Object]);

        // Act
        IModelNodeAccessor? result = provider.GetAccessor(JsonPathTokenType.Property, "Foo", new object());

        // Assert
        Assert.Same(firstMatch.Object, result);
        secondMatch.Verify(
            a => a.CanHandle(It.IsAny<JsonPathTokenType>(), It.IsAny<string>(), It.IsAny<object>()),
            Times.Never);
    }

    [Fact]
    public void GetAccessor_ReturnsNull_WhenNoAccessorCanHandle()
    {
        // Arrange
        ModelPropertyAccessorProvider provider = new([
            CreateAccessorMock(false).Object,
            CreateAccessorMock(false).Object
        ]);

        // Act
        IModelNodeAccessor? result = provider.GetAccessor(JsonPathTokenType.Property, "Foo", new object());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAccessor_ReturnsNull_WhenAccessorListIsEmpty()
    {
        // Arrange
        ModelPropertyAccessorProvider provider = new([]);

        // Act
        IModelNodeAccessor? result = provider.GetAccessor(JsonPathTokenType.Property, "Foo", new object());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAccessor_PassesTokenTypeTokenAndContainer_ToEachAccessor()
    {
        // Arrange
        Mock<IModelNodeAccessor> accessorMock = CreateAccessorMock(true);
        ModelPropertyAccessorProvider provider = new([accessorMock.Object]);
        object container = new();

        // Act
        provider.GetAccessor(JsonPathTokenType.ArrayIndex, "3", container);

        // Assert
        accessorMock.Verify(a => a.CanHandle(JsonPathTokenType.ArrayIndex, "3", container), Times.Once);
    }
}