using Microsoft.AspNetCore.Components.Forms;
using Moq;
using MRX.Json.Abstractions;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;
using MRX.UI.ErrorsAutoWiring.ModelDiscovery;

namespace MRX.UI.ErrorsAutoWiring.Tests;

// ---- Test helper types ----

/// <summary>
///     Hand-rolled fake walker (rather than a Moq mock) so tests can hand VisitModel
///     an exact, arbitrary token sequence without needing a real path string or
///     wrestling with Moq's out-parameter setup syntax.
/// </summary>
file sealed class FakeJsonPathWalker : IJsonPathWalker
{
    private readonly Queue<JsonPathToken> _tokens;

    public FakeJsonPathWalker(IEnumerable<JsonPathToken> tokens)
    {
        _tokens = new Queue<JsonPathToken>(tokens);
    }

    public bool MoveNext(out JsonPathToken token)
    {
        if (_tokens.Count == 0)
        {
            token = default;
            return false;
        }

        token = _tokens.Dequeue();
        return true;
    }
}

file sealed class SampleModel
{
    public string Name { get; set; } = string.Empty;
}

public class ModelKeyPathVisitorTests
{
    // ---- Helpers ----

    private static JsonPathToken PropertyToken(string name, bool endOfPath)
    {
        return new JsonPathToken(name, JsonPathTokenType.Property, endOfPath);
    }

    private static (Mock<IJsonPathWalkerFactory> Factory, Mock<IModelPropertyAccessorProvider> Provider)
        CreateMocks()
    {
        return (new Mock<IJsonPathWalkerFactory>(), new Mock<IModelPropertyAccessorProvider>());
    }

    private static void SetupWalker(
        Mock<IJsonPathWalkerFactory> factoryMock, string key, params JsonPathToken[] tokens)
    {
        factoryMock.Setup(f => f.Create(key)).Returns(new FakeJsonPathWalker(tokens));
    }

    // ---- Constructor ----

    [Fact]
    public void Constructor_Throws_WhenWalkerFactoryIsNull()
    {
        // Arrange
        Mock<IModelPropertyAccessorProvider> providerMock = new();

        // Act
        Action act = () => new ModelKeyPathVisitor(null!, providerMock.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Constructor_Throws_WhenAccessorProviderIsNull()
    {
        // Arrange
        Mock<IJsonPathWalkerFactory> walkerFactoryMock = new();

        // Act
        Action act = () => new ModelKeyPathVisitor(walkerFactoryMock.Object, null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    // ---- Argument validation on VisitModel(Type, object, IEnumerable<string>) ----

    [Fact]
    public void VisitModel_Throws_WhenModelTypeIsNull()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);
        SampleModel model = new();

        // Act
        Action act = () => visitor.VisitModel(null!, model, ["Name"]);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void VisitModel_Throws_WhenModelIsNull()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Action act = () => visitor.VisitModel(typeof(SampleModel), null!, ["Name"]);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void VisitModel_Throws_WhenKeysIsNull()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);
        SampleModel model = new();

        // Act
        Action act = () => visitor.VisitModel(typeof(SampleModel), model, null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void VisitModelGeneric_Throws_WhenModelIsNull()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Action act = () => visitor.VisitModel<SampleModel>(null!, ["Name"]);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void VisitModelGeneric_Throws_WhenKeysIsNull()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);
        SampleModel model = new();

        // Act
        Action act = () => visitor.VisitModel(model, null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    // ---- Empty / no-op paths ----

    [Fact]
    public void VisitModel_ReturnsEmptyDictionary_WhenKeysIsEmpty()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);
        SampleModel model = new();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(typeof(SampleModel), model, []);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void VisitModel_SkipsKey_WhenWalkerYieldsNoTokens()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(factoryMock, "Name"); // no tokens at all
        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);
        SampleModel model = new();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(typeof(SampleModel), model, ["Name"]);

        // Assert
        Assert.Empty(result);
        providerMock.Verify(
            p => p.GetAccessor(It.IsAny<JsonPathTokenType>(), It.IsAny<string>(), It.IsAny<object>()),
            Times.Never);
    }

    // ---- Accessor not found ----

    [Fact]
    public void VisitModel_SkipsKey_WhenNoAccessorFoundForLeafToken()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(factoryMock, "Name", PropertyToken("Name", true));
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", It.IsAny<object>()))
            .Returns((IModelNodeAccessor?)null);
        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);
        SampleModel model = new();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(typeof(SampleModel), model, ["Name"]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void VisitModel_SkipsKey_WhenIntermediateTokenHasNoAccessor()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(
            factoryMock,
            "Address.City",
            PropertyToken("Address", false),
            PropertyToken("City", true));
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Address", It.IsAny<object>()))
            .Returns((IModelNodeAccessor?)null);
        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);
        SampleModel model = new();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(typeof(SampleModel), model, ["Address.City"]);

        // Assert
        Assert.Empty(result);
        // The walk must have stopped at "Address" — "City" should never be requested.
        providerMock.Verify(
            p => p.GetAccessor(JsonPathTokenType.Property, "City", It.IsAny<object>()),
            Times.Never);
    }

    // ---- Intermediate resolves to null ----

    [Fact]
    public void VisitModel_SkipsKey_WhenIntermediatePropertyResolvesToNull()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(
            factoryMock,
            "Address.City",
            PropertyToken("Address", false),
            PropertyToken("City", true));

        SampleModel model = new();
        Mock<IModelNodeAccessor> addressAccessorMock = new();
        addressAccessorMock
            .Setup(a => a.GetValue("Address", model))
            .Returns((object?)null);

        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Address", model))
            .Returns(addressAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(typeof(SampleModel), model, ["Address.City"]);

        // Assert
        Assert.Empty(result);
        // Loop must have exited on the null container — "City" is never reached.
        providerMock.Verify(
            p => p.GetAccessor(JsonPathTokenType.Property, "City", It.IsAny<object>()),
            Times.Never);
    }

    // ---- Leaf accessor doesn't support naming ----

    [Fact]
    public void VisitModel_SkipsKey_WhenLeafAccessorDoesNotImplementNodeNameAccessor()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(factoryMock, "Name", PropertyToken("Name", true));

        SampleModel model = new();
        // Deliberately NOT using .As<IModelNodeNameAccessor>() here.
        Mock<IModelNodeAccessor> leafAccessorMock = new();
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(leafAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(typeof(SampleModel), model, ["Name"]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void VisitModel_SkipsKey_WhenGetNodeNameReturnsNull()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(factoryMock, "Name", PropertyToken("Name", true));

        SampleModel model = new();
        Mock<IModelNodeAccessor> leafAccessorMock = new();
        leafAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Name", model))
            .Returns((string?)null);
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(leafAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(typeof(SampleModel), model, ["Name"]);

        // Assert
        Assert.Empty(result);
    }

    // ---- Success paths ----

    [Fact]
    public void VisitModel_AddsFieldIdentifier_ForSingleSegmentPath()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(factoryMock, "Name", PropertyToken("Name", true));

        SampleModel model = new();
        Mock<IModelNodeAccessor> leafAccessorMock = new();
        leafAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Name", model))
            .Returns("Name");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(leafAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(typeof(SampleModel), model, ["Name"]);

        // Assert
        Assert.True(result.ContainsKey("Name"));
        FieldIdentifier fieldIdentifier = result["Name"];
        Assert.Same(model, fieldIdentifier.Model);
        Assert.Equal("Name", fieldIdentifier.FieldName);
    }

    [Fact]
    public void VisitModel_DoesNotDescendPastEndOfPathToken()
    {
        // Arrange
        // Confirms the EndOfPath break happens BEFORE GetValue is called on the
        // leaf accessor — the container stays the leaf's parent, not the leaf's value.
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(factoryMock, "Name", PropertyToken("Name", true));

        SampleModel model = new();
        Mock<IModelNodeAccessor> leafAccessorMock = new();
        leafAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Name", model))
            .Returns("Name");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(leafAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        visitor.VisitModel(typeof(SampleModel), model, ["Name"]);

        // Assert
        leafAccessorMock.Verify(a => a.GetValue(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public void VisitModel_AddsFieldIdentifier_ForMultiSegmentPath_UsingParentContainer()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(
            factoryMock,
            "Address.City",
            PropertyToken("Address", false),
            PropertyToken("City", true));

        SampleModel model = new();
        object addressValue = new();

        Mock<IModelNodeAccessor> addressAccessorMock = new();
        addressAccessorMock.Setup(a => a.GetValue("Address", model)).Returns(addressValue);
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Address", model))
            .Returns(addressAccessorMock.Object);

        Mock<IModelNodeAccessor> cityAccessorMock = new();
        cityAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("City", addressValue))
            .Returns("City");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "City", addressValue))
            .Returns(cityAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(typeof(SampleModel), model, ["Address.City"]);

        // Assert
        FieldIdentifier fieldIdentifier = result["Address.City"];
        // The FieldIdentifier's model must be the intermediate "Address" object
        // (the leaf's parent container), not the top-level model or a descended leaf value.
        Assert.Same(addressValue, fieldIdentifier.Model);
        Assert.Equal("City", fieldIdentifier.FieldName);
        cityAccessorMock.Verify(a => a.GetValue(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public void VisitModel_ProcessesMultipleKeysIndependently()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(factoryMock, "Name", PropertyToken("Name", true));
        SetupWalker(factoryMock, "Missing", PropertyToken("Missing", true));

        SampleModel model = new();

        Mock<IModelNodeAccessor> nameAccessorMock = new();
        nameAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Name", model))
            .Returns("Name");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(nameAccessorMock.Object);

        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Missing", model))
            .Returns((IModelNodeAccessor?)null);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result =
            visitor.VisitModel(typeof(SampleModel), model, ["Name", "Missing"]);

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey("Name"));
        Assert.False(result.ContainsKey("Missing"));
    }

    [Fact]
    public void VisitModelGeneric_ProducesSameResultAsTypeOverload()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        SetupWalker(factoryMock, "Name", PropertyToken("Name", true));

        SampleModel model = new();
        Mock<IModelNodeAccessor> leafAccessorMock = new();
        leafAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Name", model))
            .Returns("Name");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(leafAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(model, ["Name"]);

        // Assert
        Assert.True(result.ContainsKey("Name"));
        Assert.Same(model, result["Name"].Model);
        Assert.Equal("Name", result["Name"].FieldName);
    }

    // ---- Duplicate keys ----

    [Fact]
    public void VisitModel_SkipsDuplicateKey_KeepsFirstResolvedValue_WhenSameKeyAppearsTwice()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();

        // Each call to Create("Name") must hand back an independent walker instance,
        // since VisitModel walks the same key twice.
        factoryMock
            .Setup(f => f.Create("Name"))
            .Returns(() => new FakeJsonPathWalker([PropertyToken("Name", true)]));

        SampleModel model = new();
        Mock<IModelNodeAccessor> leafAccessorMock = new();
        leafAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Name", model))
            .Returns("Name");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(leafAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result =
            visitor.VisitModel(typeof(SampleModel), model, ["Name", "Name"]);

        // Assert
        // Duplicate key must not throw (Dictionary.Add would throw ArgumentException
        // on the second insert), and must not appear twice — a Dictionary can't hold
        // duplicate keys, so this also proves no exception occurred.
        Assert.Single(result);
        FieldIdentifier fieldIdentifier = result["Name"];
        Assert.Same(model, fieldIdentifier.Model);
        Assert.Equal("Name", fieldIdentifier.FieldName);
    }

    [Fact]
    public void VisitModel_DoesNotThrow_WhenSameKeyAppearsMoreThanTwice()
    {
        // Arrange
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        factoryMock
            .Setup(f => f.Create("Name"))
            .Returns(() => new FakeJsonPathWalker([PropertyToken("Name", true)]));

        SampleModel model = new();
        Mock<IModelNodeAccessor> leafAccessorMock = new();
        leafAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Name", model))
            .Returns("Name");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(leafAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Action act = () => visitor.VisitModel(typeof(SampleModel), model, ["Name", "Name", "Name"]);

        // Assert
        Exception? exception = Record.Exception(act);
        Assert.Null(exception);
    }

    [Fact]
    public void VisitModel_ProcessesRemainingKeys_AfterSkippingADuplicate()
    {
        // Arrange
        // Confirms a duplicate key doesn't abort the whole loop — a distinct key
        // appearing after the duplicate must still resolve normally.
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        factoryMock
            .Setup(f => f.Create("Name"))
            .Returns(() => new FakeJsonPathWalker([PropertyToken("Name", true)]));
        factoryMock
            .Setup(f => f.Create("Age"))
            .Returns(() => new FakeJsonPathWalker([PropertyToken("Age", true)]));

        SampleModel model = new();

        Mock<IModelNodeAccessor> nameAccessorMock = new();
        nameAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Name", model))
            .Returns("Name");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(nameAccessorMock.Object);

        Mock<IModelNodeAccessor> ageAccessorMock = new();
        ageAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Age", model))
            .Returns("Age");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Age", model))
            .Returns(ageAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result =
            visitor.VisitModel(typeof(SampleModel), model, ["Name", "Name", "Age"]);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("Name"));
        Assert.True(result.ContainsKey("Age"));
    }

    [Fact]
    public void VisitModel_SkipsSecondOccurrence_WhenFirstResolvesButKeyRepeats_EvenIfSecondWouldAlsoResolve()
    {
        // Arrange
        // Same key resolves successfully both times via distinct FakeJsonPathWalker
        // instances (Create is called once per occurrence in `keys`), but the
        // dictionary must still end up with a single entry, proving the guard
        // against duplicates runs regardless of whether re-resolution would succeed.
        (Mock<IJsonPathWalkerFactory> factoryMock, Mock<IModelPropertyAccessorProvider> providerMock) = CreateMocks();
        int walkerCreationCount = 0;
        factoryMock
            .Setup(f => f.Create("Name"))
            .Returns(() =>
            {
                walkerCreationCount++;
                return new FakeJsonPathWalker([PropertyToken("Name", true)]);
            });

        SampleModel model = new();
        Mock<IModelNodeAccessor> leafAccessorMock = new();
        leafAccessorMock.As<IModelNodeNameAccessor>()
            .Setup(a => a.GetNodeName("Name", model))
            .Returns("Name");
        providerMock
            .Setup(p => p.GetAccessor(JsonPathTokenType.Property, "Name", model))
            .Returns(leafAccessorMock.Object);

        ModelKeyPathVisitor visitor = new(factoryMock.Object, providerMock.Object);

        // Act
        Dictionary<string, FieldIdentifier> result =
            visitor.VisitModel(typeof(SampleModel), model, ["Name", "Name"]);

        // Assert
        Assert.Single(result);
    }
}