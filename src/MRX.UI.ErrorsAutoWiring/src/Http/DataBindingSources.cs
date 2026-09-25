using System.Collections;

namespace MRX.UI.ErrorsAutoWiring.Http;

/// <summary>
/// Holds the model binding sources (body and query) attached to an <see cref="Microsoft.AspNetCore.Components.Forms.EditContext"/>.
/// </summary>
public class DataBindingSources : IEnumerable<object?>
{
    /// <summary>
    /// This property holds an error that is not assignable to any more specific parameter.
    /// </summary>
    public string? RootError { get; internal set; } = string.Empty;

    /// <summary>
    /// The model bound from the request body.
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    /// The model bound from the request query string.
    /// </summary>
    public object? Query { get; set; }

    /// <summary>
    /// Enumerates the binding sources, i.e. <see cref="Body"/> followed by <see cref="Query"/>.
    /// </summary>
    /// <returns>An enumerator over the binding sources.</returns>
    public IEnumerator<object?> GetEnumerator()
    {
        yield return Body;
        yield return Query;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}