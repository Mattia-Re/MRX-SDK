using MRX.Json.Path;
using MRX.UI.ErrorsAutoWiring.Abstractions;

namespace MRX.UI.ErrorsAutoWiring.PropertyAccess;

internal class StringPropertyAccessor : IModelPropertyAccessor
{
    public bool CanHandle(JsonPathTokenType tokenType, string token, object container)
        => tokenType == JsonPathTokenType.Property;

    public object? GetValue(string token, object container)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(container);

        return container.GetType().GetProperties()
            .SingleOrDefault(p => p.Name.Equals(token, StringComparison.OrdinalIgnoreCase))
            ?.GetValue(container);
    }
}