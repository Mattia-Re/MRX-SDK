using System.Collections.Concurrent;
using System.Reflection;

namespace MRX.Parsing.Reflection;

/// <summary>
/// Invokes <see cref="GenericParser.ParseValue{T}"/> for a runtime-known type via a cached,
/// reflection-built generic method.
/// </summary>
internal static class RuntimeGenericParser
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> ParseMethodsCache = [];

    /// <summary>
    /// Parses <paramref name="value"/> into an instance of <paramref name="targetType"/> by invoking
    /// <see cref="GenericParser.ParseValue{T}"/> reflectively, caching the constructed generic method per type.
    /// </summary>
    /// <param name="targetType">The <see cref="IParsable{TSelf}"/> type to parse the value into.</param>
    /// <param name="value">The string representation of the value to parse.</param>
    /// <param name="provider">An optional format provider used to interpret <paramref name="value"/>.</param>
    /// <returns>The parsed value, boxed as <see cref="object"/>.</returns>
    internal static object Parse(Type targetType, string value, IFormatProvider? provider = null)
    {
        MethodInfo parseMethod = ParseMethodsCache.GetOrAdd(
            targetType,
            t => typeof(GenericParser)
                .GetMethod(nameof(GenericParser.ParseValue), BindingFlags.NonPublic | BindingFlags.Static)
                !.MakeGenericMethod(t));

        return parseMethod.Invoke(null, [value, provider])!;
    }
}