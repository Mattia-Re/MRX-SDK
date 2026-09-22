using System.Collections;

namespace MRX.UI.ErrorsAutoWiring.Http;

public class DataBindingSources : IEnumerable<object?>
{
    /// <summary>
    /// This property holds an error that is not assignable to any more specific parameter.
    /// </summary>
    public string? RootError { get; internal set; } = string.Empty;

    public object? Body { get; set; }
    public object? Query { get; set; }

    public IEnumerator<object?> GetEnumerator()
    {
        yield return Body;
        yield return Query;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}