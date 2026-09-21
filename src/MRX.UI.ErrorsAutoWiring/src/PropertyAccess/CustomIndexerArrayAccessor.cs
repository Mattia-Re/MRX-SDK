using System.Numerics;
using System.Reflection;
using MRX.Json.Path;
using MRX.UI.ErrorsAutoWiring.Abstractions;

namespace MRX.UI.ErrorsAutoWiring.PropertyAccess;

internal class CustomIndexerArrayAccessor(INumericTokenParser numericParser) : IModelPropertyAccessor
{
    private readonly INumericTokenParser _numericParser =
        numericParser ?? throw new ArgumentNullException(nameof(numericParser));

    public bool CanHandle(JsonPathTokenType tokenType, string token, object container)
        => tokenType == JsonPathTokenType.ArrayIndex;

    public object? GetValue(string token, object container)
    {
        // Find an indexer for numeric values
        PropertyIndexerInfo? indexerInfo = container.GetType()
            .GetProperties()
            .Select(info => new { PropertyInfo = info, IndexParameters = info.GetIndexParameters() })
            .Select(p =>
            {
                if (p.IndexParameters is not { Length: 1 } idxParams ||
                    !IsNumericType(idxParams[0].ParameterType))
                {
                    return null;
                }

                return new PropertyIndexerInfo(p.PropertyInfo, idxParams[0].ParameterType);
            })
            .SingleOrDefault(i => i != null);

        if (indexerInfo == null)
            throw new InvalidOperationException(
                $"Property {container.GetType().FullName} does not implement an indexer or enumerator");

        object numericToken = _numericParser.Parse(indexerInfo.IndexerType, token);

        try
        {
            return indexerInfo.PropertyInfo.GetValue(container, [numericToken]);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is IndexOutOfRangeException)
        {
            return null;
        }
    }

    private static bool IsNumericType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        // Check if type implements INumber<type>
        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>) &&
            i.GetGenericArguments()[0] == type);
    }
}