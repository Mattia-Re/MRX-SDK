namespace MRX.UI.ErrorsAutoWiring.Blazor.Tests.Models;

/// <summary>
///     A plain data model used to exercise nested-object (<c>obj.data</c>) and array-indexer
///     (<c>obj.cars[0].name</c>) JSON path keys against a real <c>EditForm</c>.
/// </summary>
public class SampleRequest
{
    public SampleObj Obj { get; set; } = new();
}

public class SampleObj
{
    public string? Data { get; set; }

    public List<SampleCar> Cars { get; set; } = [new()];
}

public class SampleCar
{
    public string? Name { get; set; }
}