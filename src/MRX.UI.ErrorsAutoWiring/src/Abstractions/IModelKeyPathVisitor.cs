using Microsoft.AspNetCore.Components.Forms;

namespace MRX.UI.ErrorsAutoWiring.Abstractions;

/// <summary>
/// Resolves a set of JSON path keys against a model, returning the field each key ultimately points at.
/// </summary>
public interface IModelKeyPathVisitor
{
    /// <summary>
    /// Scans <paramref name="model"/> for fields referenced by <paramref name="keys"/>.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="model">The model to scan for the keys.</param>
    /// <param name="keys">All field keys to scan for.</param>
    /// <returns>A dictionary linking each key to the corresponding field.</returns>
    Dictionary<string, FieldIdentifier> VisitModel<T>(T model, IEnumerable<string> keys);

    /// <summary>
    /// Scans <paramref name="model"/> for fields referenced by <paramref name="keys"/>.
    /// </summary>
    /// <param name="modelType">The runtime type of <paramref name="model"/>.</param>
    /// <param name="model">The model to scan for the keys.</param>
    /// <param name="keys">All field keys to scan for.</param>
    /// <returns>A dictionary linking each key to the corresponding field.</returns>
    Dictionary<string, FieldIdentifier> VisitModel(Type modelType, object model, IEnumerable<string> keys);
}