using System.Collections.Concurrent;
using System.Reflection;

namespace MRX.Json.Parsing;

internal static class RuntimeGenericParser
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> ParseMethodsCache = [];

    internal static object Parse(Type numericType, string value, IFormatProvider? provider = null)
    {
        MethodInfo parseMethod = ParseMethodsCache.GetOrAdd(
            numericType,
            t => typeof(GenericParser)
                .GetMethod(nameof(GenericParser.ParseValue), BindingFlags.NonPublic | BindingFlags.Static)
                !.MakeGenericMethod(t));

        return parseMethod.Invoke(null, [value, provider])!;
    }
}