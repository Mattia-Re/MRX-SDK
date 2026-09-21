using System.Collections;
using System.Diagnostics;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;

namespace MRX.Json.Reflection.PropertyAccess;

internal class ArrayElementAccessor : IModelNodeAccessor
{
    public bool CanHandle(JsonPathTokenType tokenType, string token, object container)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(container);

        if (tokenType != JsonPathTokenType.ArrayIndex) return false;
        return container is IList or IEnumerable;
    }

    public object? GetValue(string token, object container)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(container);

        if (container is IList list)
        {
            int index = int.Parse(token);
            return index > list.Count - 1 ? null : list[index];
        }

        if (container is IEnumerable enumerable)
        {
            int index = int.Parse(token);
            return enumerable.Cast<object>().ElementAtOrDefault(index);
        }

        throw new UnreachableException();
    }
}