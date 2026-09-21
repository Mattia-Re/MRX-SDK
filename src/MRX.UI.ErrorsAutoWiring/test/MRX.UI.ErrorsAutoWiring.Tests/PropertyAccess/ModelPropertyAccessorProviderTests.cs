using Moq;
using MRX.Json.Path;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.PropertyAccess;

namespace MRX.UI.ErrorsAutoWiring.Tests.PropertyAccess;

public class ModelPropertyAccessorProviderTests
{
    private static Mock<IModelPropertyAccessor> CreateAccessorMock(bool canHandle)
    {
        Mock<IModelPropertyAccessor> mock = new();
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
        Mock<IModelPropertyAccessor> noMatch = CreateAccessorMock(false);
        Mock<IModelPropertyAccessor> match = CreateAccessorMock(true);
        ModelPropertyAccessorProvider provider = new([noMatch.Object, match.Object]);

        // Act
        IModelPropertyAccessor? result = provider.GetAccessor(JsonPathTokenType.ArrayIndex, "Foo", new object());

        // Assert
        Assert.Same(match.Object, result);
    }

    [Fact]
    public void GetAccessor_StopsAtFirstMatch_IgnoresLaterMatches()
    {
        // Arrange
        Mock<IModelPropertyAccessor> firstMatch = CreateAccessorMock(true);
        Mock<IModelPropertyAccessor> secondMatch = CreateAccessorMock(true);
        ModelPropertyAccessorProvider provider = new([firstMatch.Object, secondMatch.Object]);

        // Act
        IModelPropertyAccessor? result = provider.GetAccessor(JsonPathTokenType.Property, "Foo", new object());

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
        IModelPropertyAccessor? result = provider.GetAccessor(JsonPathTokenType.Property, "Foo", new object());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAccessor_ReturnsNull_WhenAccessorListIsEmpty()
    {
        // Arrange
        ModelPropertyAccessorProvider provider = new([]);

        // Act
        IModelPropertyAccessor? result = provider.GetAccessor(JsonPathTokenType.Property, "Foo", new object());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAccessor_PassesTokenTypeTokenAndContainer_ToEachAccessor()
    {
        // Arrange
        Mock<IModelPropertyAccessor> accessorMock = CreateAccessorMock(true);
        ModelPropertyAccessorProvider provider = new([accessorMock.Object]);
        object container = new();

        // Act
        provider.GetAccessor(JsonPathTokenType.ArrayIndex, "3", container);

        // Assert
        accessorMock.Verify(a => a.CanHandle(JsonPathTokenType.ArrayIndex, "3", container), Times.Once);
    }
}