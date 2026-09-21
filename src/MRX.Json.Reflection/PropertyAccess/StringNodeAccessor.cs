using System.Reflection;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;

namespace MRX.Json.Reflection.PropertyAccess;

internal class StringNodeAccessor : IModelNodeAccessor, IModelNodeNameAccessor
{
    public bool CanHandle(JsonPathTokenType tokenType, string token, object container)
        => tokenType == JsonPathTokenType.Property;

    public object? GetValue(string token, object container)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(container);

        return GetProperty(token, container)?.GetValue(container);
    }

    public string? GetNodeName(string token, object container)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(container);

        return GetProperty(token, container)?.Name;
    }

    private static PropertyInfo? GetProperty(string token, object container)
    {
        return container.GetType().GetProperties()
            .SingleOrDefault(p => p.Name.Equals(token, StringComparison.OrdinalIgnoreCase));
    }
}