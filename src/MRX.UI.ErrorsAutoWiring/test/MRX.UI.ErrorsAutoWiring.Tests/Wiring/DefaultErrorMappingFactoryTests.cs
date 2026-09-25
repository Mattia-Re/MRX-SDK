using Microsoft.AspNetCore.Components.Forms;
using Moq;
using MRX.Core.Common.ModelBinding;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.Http;
using MRX.UI.ErrorsAutoWiring.Wiring;

namespace MRX.UI.ErrorsAutoWiring.Tests.Wiring;

// ---- Test model shapes ----

file sealed class BodyModel
{
    public string Name { get; set; } = string.Empty;
}

file sealed class QueryModel
{
    public string Filter { get; set; } = string.Empty;
}

file sealed class PathModel
{
    public string Id { get; set; } = string.Empty;
}

file sealed class HeaderModel
{
    public string Token { get; set; } = string.Empty;
}

public class DefaultErrorMappingFactoryTests
{
    // ---- Helpers ----

    private static HttpResponseMessage CreateResponse(params string[]? disambiguatedHeaderValues)
    {
        HttpResponseMessage response = new();
        if (disambiguatedHeaderValues != null)
        {
            response.Headers.TryAddWithoutValidation("X-Disambiguated", disambiguatedHeaderValues);
        }

        return response;
    }

    private static EditContext CreateEditContext(object model) => new(model);

    // ---- Model type guard ----

    [Fact]
    public void CreateErrorFieldMap_Throws_WhenModelIsNotDataBindingSources()
    {
        // Arrange
        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        EditContext editContext = CreateEditContext(new object());
        Dictionary<string, CodedError[]> errors = [];
        HttpResponseMessage response = CreateResponse("true");

        // Act
        Action act = () => factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    // ---- Header validation ----

    [Fact]
    public void CreateErrorFieldMap_Throws_WhenDisambiguatedHeaderIsMissing()
    {
        // Arrange
        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        DataBindingSources sources = new() { Body = new BodyModel(), Query = new QueryModel() };
        EditContext editContext = CreateEditContext(sources);
        Dictionary<string, CodedError[]> errors = [];
        HttpResponseMessage response = CreateResponse(disambiguatedHeaderValues: null);

        // Act
        Action act = () => factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void CreateErrorFieldMap_Throws_WhenDisambiguatedHeaderHasMultipleValues()
    {
        // Arrange
        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        DataBindingSources sources = new() { Body = new BodyModel(), Query = new QueryModel() };
        EditContext editContext = CreateEditContext(sources);
        Dictionary<string, CodedError[]> errors = [];
        HttpResponseMessage response = CreateResponse("true", "false");

        // Act
        Action act = () => factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void CreateErrorFieldMap_Throws_WhenDisambiguatedHeaderValueIsNotABoolean()
    {
        // Arrange
        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        DataBindingSources sources = new() { Body = new BodyModel(), Query = new QueryModel() };
        EditContext editContext = CreateEditContext(sources);
        Dictionary<string, CodedError[]> errors = [];
        HttpResponseMessage response = CreateResponse("maybe");

        // Act
        Action act = () => factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    // ---- Disambiguated = true ----

    [Fact]
    public void CreateErrorFieldMap_MapsErrorsToFieldIdentifiers_WhenDisambiguatedAndKeysMatchKnownSources()
    {
        // Arrange
        BodyModel bodyModel = new();
        QueryModel queryModel = new();
        PathModel pathModel = new();
        HeaderModel headerModel = new();
        DataBindingSources sources = new()
        {
            Body = bodyModel, Query = queryModel, Path = pathModel, Header = headerModel,
        };
        EditContext editContext = CreateEditContext(sources);

        CodedError bodyError = new("E1", "Name is required");
        CodedError queryError = new("E2", "Filter is invalid");
        CodedError pathError = new("E3", "Id is invalid");
        CodedError headerError = new("E4", "Token is invalid");
        Dictionary<string, CodedError[]> errors = new()
        {
            ["body.name"] = [bodyError],
            ["query.filter"] = [queryError],
            ["path.id"] = [pathError],
            ["header.token"] = [headerError],
        };

        FieldIdentifier bodyFieldId = new(bodyModel, "Name");
        FieldIdentifier queryFieldId = new(queryModel, "Filter");
        FieldIdentifier pathFieldId = new(pathModel, "Id");
        FieldIdentifier headerFieldId = new(headerModel, "Token");

        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(BodyModel), bodyModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(new List<string> { "name" }))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["name"] = bodyFieldId });
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(QueryModel), queryModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(new List<string> { "filter" }))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["filter"] = queryFieldId });
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(PathModel), pathModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(new List<string> { "id" }))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["id"] = pathFieldId });
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(HeaderModel), headerModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(new List<string> { "token" }))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["token"] = headerFieldId });

        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        HttpResponseMessage response = CreateResponse("true");

        // Act
        Dictionary<FieldIdentifier, List<CodedError>> result =
            factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Same(bodyError, Assert.Single(result[bodyFieldId]));
        Assert.Same(queryError, Assert.Single(result[queryFieldId]));
        Assert.Same(pathError, Assert.Single(result[pathFieldId]));
        Assert.Same(headerError, Assert.Single(result[headerFieldId]));
    }

    [Fact]
    public void CreateErrorFieldMap_SilentlySkipsErrors_WhenPrefixMatchesNoKnownSource()
    {
        // Arrange
        DataBindingSources sources = new() { Body = new BodyModel(), Query = new QueryModel() };
        EditContext editContext = CreateEditContext(sources);
        Dictionary<string, CodedError[]> errors = new()
        {
            ["cookie.token"] = [new CodedError("E9", "Unrecognized source")],
        };

        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        HttpResponseMessage response = CreateResponse("true");

        // Act
        Dictionary<FieldIdentifier, List<CodedError>> result =
            factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Empty(result);
        modelVisitorMock.Verify(
            v => v.VisitModel(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
    }

    [Fact]
    public void CreateErrorFieldMap_MapsOnlyKnownSourceKeys_WhenMixedWithUnknownPrefixes()
    {
        // Arrange
        BodyModel bodyModel = new();
        DataBindingSources sources = new() { Body = bodyModel, Query = new QueryModel() };
        EditContext editContext = CreateEditContext(sources);

        CodedError bodyError = new("E1", "Name is required");
        Dictionary<string, CodedError[]> errors = new()
        {
            ["body.name"] = [bodyError],
            ["cookie.token"] = [new CodedError("E9", "Unrecognized source")],
        };

        FieldIdentifier bodyFieldId = new(bodyModel, "Name");
        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(BodyModel), bodyModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(new List<string> { "name" }))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["name"] = bodyFieldId });

        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        HttpResponseMessage response = CreateResponse("true");

        // Act
        Dictionary<FieldIdentifier, List<CodedError>> result =
            factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Single(result);
        Assert.Same(bodyError, Assert.Single(result[bodyFieldId]));
    }

    [Fact]
    public void CreateErrorFieldMap_MatchesSourcePrefixCaseInsensitively_WhenDisambiguated()
    {
        // Arrange
        BodyModel bodyModel = new();
        DataBindingSources sources = new() { Body = bodyModel, Query = new QueryModel() };
        EditContext editContext = CreateEditContext(sources);

        CodedError bodyError = new("E1", "Name is required");
        Dictionary<string, CodedError[]> errors = new() { ["Body.Name"] = [bodyError] };

        FieldIdentifier bodyFieldId = new(bodyModel, "Name");
        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(BodyModel), bodyModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(new List<string> { "Name" }))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["Name"] = bodyFieldId });

        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        HttpResponseMessage response = CreateResponse("true");

        // Act
        Dictionary<FieldIdentifier, List<CodedError>> result =
            factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Single(result);
        Assert.Same(bodyError, Assert.Single(result[bodyFieldId]));
    }

    // ---- Disambiguated = false ----

    [Fact]
    public void CreateErrorFieldMap_ScansAllSourcesWithFullKeySet_WhenNotDisambiguated()
    {
        // Arrange
        BodyModel bodyModel = new();
        QueryModel queryModel = new();
        PathModel pathModel = new();
        HeaderModel headerModel = new();
        DataBindingSources sources = new()
        {
            Body = bodyModel, Query = queryModel, Path = pathModel, Header = headerModel,
        };
        EditContext editContext = CreateEditContext(sources);

        CodedError nameError = new("E1", "Name is required");
        CodedError filterError = new("E2", "Filter is invalid");
        CodedError idError = new("E3", "Id is invalid");
        CodedError tokenError = new("E4", "Token is invalid");
        Dictionary<string, CodedError[]> errors = new()
        {
            ["name"] = [nameError],
            ["filter"] = [filterError],
            ["id"] = [idError],
            ["token"] = [tokenError],
        };

        List<string> allKeys = ["name", "filter", "id", "token"];

        FieldIdentifier bodyFieldId = new(bodyModel, "Name");
        FieldIdentifier queryFieldId = new(queryModel, "Filter");
        FieldIdentifier pathFieldId = new(pathModel, "Id");
        FieldIdentifier headerFieldId = new(headerModel, "Token");

        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(BodyModel), bodyModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(allKeys))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["name"] = bodyFieldId });
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(QueryModel), queryModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(allKeys))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["filter"] = queryFieldId });
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(PathModel), pathModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(allKeys))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["id"] = pathFieldId });
        modelVisitorMock
            .Setup(v => v.VisitModel(
                typeof(HeaderModel), headerModel,
                It.Is<IEnumerable<string>>(k => k.SequenceEqual(allKeys))))
            .Returns(new Dictionary<string, FieldIdentifier> { ["token"] = headerFieldId });

        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        HttpResponseMessage response = CreateResponse("false");

        // Act
        Dictionary<FieldIdentifier, List<CodedError>> result =
            factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Same(nameError, Assert.Single(result[bodyFieldId]));
        Assert.Same(filterError, Assert.Single(result[queryFieldId]));
        Assert.Same(idError, Assert.Single(result[pathFieldId]));
        Assert.Same(tokenError, Assert.Single(result[headerFieldId]));
    }

    [Fact]
    public void CreateErrorFieldMap_SkipsNullSource_WhenNotDisambiguated()
    {
        // Arrange
        BodyModel bodyModel = new();
        DataBindingSources sources = new() { Body = bodyModel, Query = null };
        EditContext editContext = CreateEditContext(sources);

        Dictionary<string, CodedError[]> errors = new() { ["name"] = [new CodedError("E1", "Name is required")] };

        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        modelVisitorMock
            .Setup(v => v.VisitModel(typeof(BodyModel), bodyModel, It.IsAny<IEnumerable<string>>()))
            .Returns(new Dictionary<string, FieldIdentifier> { ["name"] = new FieldIdentifier(bodyModel, "Name") });

        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        HttpResponseMessage response = CreateResponse("false");

        // Act
        factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        modelVisitorMock.Verify(
            v => v.VisitModel(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<IEnumerable<string>>()),
            Times.Once);
    }

    // ---- FAILSAFE: colliding FieldIdentifiers must have their errors merged ----

    [Fact]
    public void CreateErrorFieldMap_MergesErrors_WhenTwoDifferentKeysResolveToSameFieldIdentifier()
    {
        // Arrange
        // FAILSAFE EXPECTATION (per ADR-003): two distinct path keys resolving to the
        // same FieldIdentifier must not crash — their errors are merged under that
        // single FieldIdentifier rather than either being dropped.
        BodyModel bodyModel = new();
        DataBindingSources sources = new() { Body = bodyModel, Query = new QueryModel() };
        EditContext editContext = CreateEditContext(sources);

        CodedError firstError = new("E1", "err1");
        CodedError secondError = new("E2", "err2");
        Dictionary<string, CodedError[]> errors = new()
        {
            ["body.name"] = [firstError],
            ["body.nickname"] = [secondError],
        };

        FieldIdentifier sharedFieldId = new(bodyModel, "Name");
        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        modelVisitorMock
            .Setup(v => v.VisitModel(typeof(BodyModel), bodyModel, It.IsAny<IEnumerable<string>>()))
            .Returns(new Dictionary<string, FieldIdentifier>
            {
                ["name"] = sharedFieldId,
                ["nickname"] = sharedFieldId,
            });

        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        HttpResponseMessage response = CreateResponse("true");

        // Act
        Dictionary<FieldIdentifier, List<CodedError>> result =
            factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[sharedFieldId].Count);
        Assert.Contains(firstError, result[sharedFieldId]);
        Assert.Contains(secondError, result[sharedFieldId]);
    }

    [Fact]
    public void CreateErrorFieldMap_MergesErrors_WhenTwoDifferentSourcesResolveToSameFieldIdentifier()
    {
        // Arrange
        // FAILSAFE EXPECTATION (per ADR-003): Body and Query resolving to the same
        // underlying FieldIdentifier must merge, not overwrite or crash.
        BodyModel sharedModel = new();
        DataBindingSources sources = new() { Body = sharedModel, Query = sharedModel };
        EditContext editContext = CreateEditContext(sources);

        CodedError bodyError = new("E1", "err1");
        CodedError queryError = new("E2", "err2");
        Dictionary<string, CodedError[]> errors = new()
        {
            ["body.name"] = [bodyError],
            ["query.name"] = [queryError],
        };

        FieldIdentifier sharedFieldId = new(sharedModel, "Name");
        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        modelVisitorMock
            .Setup(v => v.VisitModel(typeof(BodyModel), sharedModel, It.IsAny<IEnumerable<string>>()))
            .Returns(new Dictionary<string, FieldIdentifier> { ["name"] = sharedFieldId });

        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        HttpResponseMessage response = CreateResponse("true");

        // Act
        Dictionary<FieldIdentifier, List<CodedError>> result =
            factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[sharedFieldId].Count);
        Assert.Contains(bodyError, result[sharedFieldId]);
        Assert.Contains(queryError, result[sharedFieldId]);
    }

    [Fact]
    public void CreateErrorFieldMap_MergesErrors_WhenSameSourceKeyCollisionHasMultipleErrorsEach()
    {
        // Arrange
        // Confirms merging concatenates full arrays (not just single errors) —
        // each side already carrying multiple CodedError entries.
        BodyModel bodyModel = new();
        DataBindingSources sources = new() { Body = bodyModel, Query = new QueryModel() };
        EditContext editContext = CreateEditContext(sources);

        CodedError e1 = new("E1", "err1");
        CodedError e2 = new("E2", "err2");
        CodedError e3 = new("E3", "err3");
        Dictionary<string, CodedError[]> errors = new()
        {
            ["body.name"] = [e1, e2],
            ["body.nickname"] = [e3],
        };

        FieldIdentifier sharedFieldId = new(bodyModel, "Name");
        Mock<IModelKeyPathVisitor> modelVisitorMock = new();
        modelVisitorMock
            .Setup(v => v.VisitModel(typeof(BodyModel), bodyModel, It.IsAny<IEnumerable<string>>()))
            .Returns(new Dictionary<string, FieldIdentifier>
            {
                ["name"] = sharedFieldId,
                ["nickname"] = sharedFieldId,
            });

        DefaultErrorMappingFactory factory = new(modelVisitorMock.Object);
        HttpResponseMessage response = CreateResponse("true");

        // Act
        Dictionary<FieldIdentifier, List<CodedError>> result =
            factory.CreateErrorFieldMap(editContext, errors, response);

        // Assert
        Assert.Equal(3, result[sharedFieldId].Count);
        Assert.Contains(e1, result[sharedFieldId]);
        Assert.Contains(e2, result[sharedFieldId]);
        Assert.Contains(e3, result[sharedFieldId]);
    }
}