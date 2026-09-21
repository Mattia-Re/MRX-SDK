using Microsoft.AspNetCore.Components.Forms;
using MRX.Json.Abstractions;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;

namespace MRX.UI.ErrorsAutoWiring.ModelDiscovery;

/// <summary>
///     Walks a model along a set of JSON path keys, resolving each key to the field it ultimately points at.
/// </summary>
internal class ModelKeyPathVisitor(
    IJsonPathWalkerFactory walkerFactory,
    IModelPropertyAccessorProvider accessorProvider)
{
    private readonly IJsonPathWalkerFactory _walkerFactory =
        walkerFactory ?? throw new ArgumentNullException(nameof(walkerFactory));

    private readonly IModelPropertyAccessorProvider _accessorProvider =
        accessorProvider ?? throw new ArgumentNullException(nameof(accessorProvider));

    /// <summary>
    ///     Scans the model for fields referenced by keys.
    /// </summary>
    /// <param name="model">The model to scan for the keys</param>
    /// <param name="keys">All field keys to scan for</param>
    /// <returns>A dictionary linking each key to the corresponding field.</returns>
    internal Dictionary<string, FieldIdentifier> VisitModel<T>(T model, IEnumerable<string> keys)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(keys);

        return VisitModel(typeof(T), model, keys);
    }

    internal Dictionary<string, FieldIdentifier> VisitModel(Type modelType, object model, IEnumerable<string> keys)
    {
        ArgumentNullException.ThrowIfNull(modelType);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(keys);

        Dictionary<string, FieldIdentifier> fields = [];

        // We attempt to find each leaf in the model, eventually building a FieldIdentifier for it.
        // Whenever a node can't be found, we just bail and skip this key
        foreach (string key in keys)
        {
            IJsonPathWalker walker = _walkerFactory.Create(key);

            // We keep track of the walk state so that we can break on the last token before updating the container to
            // it. 
            object? container = model;
            string tokenValue = null!;
            IModelNodeAccessor? nodeAccessor = null;

            while (container != null && walker.MoveNext(out JsonPathToken token))
            {
                tokenValue = token.Token;
                nodeAccessor =
                    _accessorProvider.GetAccessor(token.Type, tokenValue, container);

                if (token.EndOfPath) break;
                if (nodeAccessor == null) break;

                container = nodeAccessor.GetValue(tokenValue, container);
            }

            // We expect the leaf to be a node for which an accessor capable of getting the node name was used
            if (container == null || nodeAccessor == null) continue;
            if (nodeAccessor is not IModelNodeNameAccessor nodeNameAccessor) continue;

            string? leafName = nodeNameAccessor.GetNodeName(tokenValue, container);
            if (leafName == null) continue;

            FieldIdentifier fieldIdentifier = new(container, leafName);

            // Failsafe per ADR-003: upstream API contract (ADR-001) forbids duplicate keys, but this client must
            // not fault if that contract is violated.
            fields.TryAdd(key, fieldIdentifier);
        }

        return fields;
    }
}