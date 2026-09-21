using System.Numerics;
using MRX.Parsing.Reflection.Abstractions;

namespace MRX.Parsing.Reflection;

public class NumericParser : INumericParser
{
    public object Parse(Type numericType, string token, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(numericType);
        ArgumentNullException.ThrowIfNull(token);

        if (!numericType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>)))
            throw new InvalidOperationException(
                $"{numericType.FullName} does not implement {typeof(INumber<>).FullName}");

        return RuntimeGenericParser.Parse(numericType, token, provider);
    }
}