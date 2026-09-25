using System.Numerics;
using MRX.Parsing.Reflection.Abstractions;

namespace MRX.Parsing.Reflection;

/// <summary>
/// Default <see cref="INumericParser"/> implementation that parses tokens via reflection over
/// types implementing <see cref="INumber{TSelf}"/>.
/// </summary>
public class NumericParser : INumericParser
{
    /// <summary>
    /// Parses <paramref name="token"/> into a value of <paramref name="numericType"/>.
    /// </summary>
    /// <param name="numericType">The numeric type to parse the token into. Must implement <see cref="INumber{TSelf}"/>.</param>
    /// <param name="token">The string representation of the value to parse.</param>
    /// <param name="provider">An optional format provider used to interpret <paramref name="token"/>.</param>
    /// <returns>The parsed value, boxed as <see cref="object"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="numericType"/> or <paramref name="token"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="numericType"/> does not implement <see cref="INumber{TSelf}"/>.</exception>
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