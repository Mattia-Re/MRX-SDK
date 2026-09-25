using System.Collections;

namespace MRX.UI.ErrorsAutoWiring.Http;

/// <summary>
/// Holds the model binding sources (body, query, path and header) attached to an <see cref="Microsoft.AspNetCore.Components.Forms.EditContext"/>.
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
    /// The model bound from the request's route values.
    /// </summary>
    public object? Path { get; set; }

    /// <summary>
    /// The model bound from the request headers.
    /// </summary>
    public object? Header { get; set; }

    /// <summary>
    /// Enumerates the binding sources, i.e. <see cref="Body"/>, <see cref="Query"/>, <see cref="Path"/> and
    /// <see cref="Header"/>, in that order.
    /// </summary>
    /// <returns>An enumerator over the binding sources.</returns>
    public IEnumerator<object?> GetEnumerator()
    {
        yield return Body;
        yield return Query;
        yield return Path;
        yield return Header;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}