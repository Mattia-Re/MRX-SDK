namespace MRX.UI.ErrorsAutoWiring.Http;

public class DataBindingSources
{
    /// <summary>
    /// This property holds an error that is not assignable to any more specific parameter.
    /// </summary>
    public string? RootError { get; internal set; } = string.Empty;

    public object? Body { get; set; }
    public object? Query { get; set; }
}