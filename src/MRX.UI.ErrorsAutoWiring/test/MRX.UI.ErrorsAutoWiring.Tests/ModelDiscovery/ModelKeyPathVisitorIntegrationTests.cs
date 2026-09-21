using Microsoft.AspNetCore.Components.Forms;
using MRX.Json.Abstractions;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;
using MRX.Json.Reflection.PropertyAccess;
using MRX.Parsing.Reflection;
using MRX.UI.ErrorsAutoWiring.ModelDiscovery;

namespace MRX.UI.ErrorsAutoWiring.Tests.ModelDiscovery;

// ---- Test model shapes, matching the path examples under test ----

internal sealed class DataItem
{
    public string Text { get; set; } = string.Empty;
}

internal sealed class ObjModel
{
    public List<DataItem> Data { get; set; } = [];
    public List<List<DataItem>> Data2 { get; set; } = [];
}

internal sealed class RootModel
{
    public ObjModel Obj { get; set; } = new();
}

public class ModelKeyPathVisitorJsonPathIntegrationTests
{
    // ---- Helpers ----

    private static ModelKeyPathVisitor CreateVisitorWithRealPipeline()
    {
        IJsonPathWalkerFactory walkerFactory = new JsonPathWalkerFactory();
        IModelPropertyAccessorProvider provider = new ModelPropertyAccessorProvider([
            new StringNodeAccessor(),
            new ArrayElementAccessor(),
            new CustomIndexerArrayAccessor(new NumericParser())
        ]);
        return new ModelKeyPathVisitor(walkerFactory, provider);
    }

    private static RootModel CreateSampleModel()
    {
        DataItem first = new() { Text = "first" };
        DataItem second = new() { Text = "second" };
        DataItem nested = new() { Text = "nested" };

        return new RootModel
        {
            Obj = new ObjModel
            {
                Data = [first, second],
                Data2 = [[], [nested]]
            }
        };
    }

    // ---- Simple property path ----

    [Fact]
    public void VisitModel_ResolvesFieldIdentifier_ForSimplePropertyPath()
    {
        // Arrange
        ModelKeyPathVisitor visitor = CreateVisitorWithRealPipeline();
        RootModel model = CreateSampleModel();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(model, ["obj.data"]);

        // Assert
        Assert.True(result.ContainsKey("obj.data"));
        FieldIdentifier fieldIdentifier = result["obj.data"];
        Assert.Same(model.Obj, fieldIdentifier.Model);
        Assert.Equal("Data", fieldIdentifier.FieldName);
    }

    // ---- Bare array-index path (no trailing property) ----

    [Fact]
    public void VisitModel_SkipsPath_WhenPathEndsInArrayIndexWithNoTrailingProperty()
    {
        // Arrange
        // ArrayElementAccessor implements IModelNodeAccessor but not
        // IModelNodeNameAccessor, so a path terminating on an index has no
        // way to produce a field name and must be skipped, not throw.
        ModelKeyPathVisitor visitor = CreateVisitorWithRealPipeline();
        RootModel model = CreateSampleModel();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(model, ["obj.data[1]"]);

        // Assert
        Assert.Empty(result);
    }

    // ---- Array index followed by a property ----

    [Fact]
    public void VisitModel_ResolvesFieldIdentifier_ForArrayIndexFollowedByProperty()
    {
        // Arrange
        ModelKeyPathVisitor visitor = CreateVisitorWithRealPipeline();
        RootModel model = CreateSampleModel();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(model, ["obj.data[1].text"]);

        // Assert
        Assert.True(result.ContainsKey("obj.data[1].text"));
        FieldIdentifier fieldIdentifier = result["obj.data[1].text"];
        // container should be the DataItem at index 1 ("second"), not model.Obj
        // or the top-level model — confirming descent through the array element.
        Assert.Same(model.Obj.Data[1], fieldIdentifier.Model);
        Assert.Equal("Text", fieldIdentifier.FieldName);
    }

    // ---- Nested (double) array index, no trailing property ----

    [Fact]
    public void VisitModel_SkipsPath_WhenNestedArrayPathEndsInIndexWithNoTrailingProperty()
    {
        // Arrange
        ModelKeyPathVisitor visitor = CreateVisitorWithRealPipeline();
        RootModel model = CreateSampleModel();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(model, ["obj.data2[1][0]"]);

        // Assert
        Assert.Empty(result);
    }

    // ---- Nested (double) array index followed by a property ----

    [Fact]
    public void VisitModel_ResolvesFieldIdentifier_ForNestedArrayIndexFollowedByProperty()
    {
        // Arrange
        ModelKeyPathVisitor visitor = CreateVisitorWithRealPipeline();
        RootModel model = CreateSampleModel();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(model, ["obj.data2[1][0].text"]);

        // Assert
        Assert.True(result.ContainsKey("obj.data2[1][0].text"));
        FieldIdentifier fieldIdentifier = result["obj.data2[1][0].text"];
        Assert.Same(model.Obj.Data2[1][0], fieldIdentifier.Model);
        Assert.Equal("Text", fieldIdentifier.FieldName);
    }

    // ---- All example paths processed together ----

    [Fact]
    public void VisitModel_ResolvesOnlyPropertyTerminatedPaths_WhenAllExamplePathsRequestedTogether()
    {
        // Arrange
        // Combines every path shape from the requirement in a single call, to
        // confirm they don't interfere with one another (e.g. shared walker
        // factory calls, shared container state) and that exactly the
        // property-terminated ones resolve.
        ModelKeyPathVisitor visitor = CreateVisitorWithRealPipeline();
        RootModel model = CreateSampleModel();
        string[] keys =
        [
            "obj.data[1]",
            "obj.data",
            "obj.data[1].text",
            "obj.data2[1][0]",
            "obj.data2[1][0].text"
        ];

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(model, keys);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result.ContainsKey("obj.data"));
        Assert.True(result.ContainsKey("obj.data[1].text"));
        Assert.True(result.ContainsKey("obj.data2[1][0].text"));
        Assert.False(result.ContainsKey("obj.data[1]"));
        Assert.False(result.ContainsKey("obj.data2[1][0]"));
    }

    // ---- Index out of bounds within a real path ----

    [Fact]
    public void VisitModel_SkipsPath_WhenArrayIndexInPathIsOutOfBounds()
    {
        // Arrange
        // model.Obj.Data has 2 elements (indices 0-1); index 5 is out of range.
        // ArrayElementAccessor.GetValue returns null for an out-of-bounds index,
        // which the traversal loop treats as "container is null" and exits early.
        ModelKeyPathVisitor visitor = CreateVisitorWithRealPipeline();
        RootModel model = CreateSampleModel();

        // Act
        Dictionary<string, FieldIdentifier> result = visitor.VisitModel(model, ["obj.data[5].text"]);

        // Assert
        Assert.Empty(result);
    }
}