using System.Numerics;
using System.Reflection;
using MRX.Json.Path;
using MRX.Json.Reflection.Abstractions;
using MRX.Parsing.Reflection.Abstractions;

namespace MRX.Json.Reflection.PropertyAccess;

/// <summary>
/// Resolves numeric array-index tokens against containers exposing a custom indexer property with a numeric parameter.
/// </summary>
/// <param name="numericParser">Parses the raw token text into the indexer's numeric parameter type.</param>
internal class CustomIndexerArrayAccessor(INumericParser numericParser) : IModelNodeAccessor
{
    private readonly INumericParser _numericParser =
        numericParser ?? throw new ArgumentNullException(nameof(numericParser));

    /// <summary>
    /// Determines whether <paramref name="tokenType"/> is an array index; the actual indexer lookup happens in <see cref="GetValue"/>.
    /// </summary>
    /// <param name="tokenType">The kind of path token being resolved.</param>
    /// <param name="token">The raw token text extracted from the JSON path.</param>
    /// <param name="container">The object instance the token should be resolved against.</param>
    /// <returns><see langword="true"/> if <paramref name="tokenType"/> is <see cref="JsonPathTokenType.ArrayIndex"/>.</returns>
    public bool CanHandle(JsonPathTokenType tokenType, string token, object container)
        => tokenType == JsonPathTokenType.ArrayIndex;

    /// <summary>
    /// Finds the container's numeric indexer property, parses <paramref name="token"/> into its parameter type,
    /// and invokes it.
    /// </summary>
    /// <param name="token">The array index, as a numeric string.</param>
    /// <param name="container">The object exposing a numeric indexer property.</param>
    /// <returns>The value returned by the indexer, or <see langword="null"/> if the index is out of range.</returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="container"/>'s type does not declare a single-parameter numeric indexer.
    /// </exception>
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

    /// <summary>
    /// Determines whether <paramref name="type"/> (or its underlying type, if nullable) implements
    /// <see cref="INumber{TSelf}"/>.
    /// </summary>
    /// <param name="type">The type to test.</param>
    /// <returns><see langword="true"/> if the type is numeric; otherwise, <see langword="false"/>.</returns>
    private static bool IsNumericType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        // Check if type implements INumber<type>
        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>) &&
            i.GetGenericArguments()[0] == type);
    }
}