namespace MRX.Parsing.Reflection.Abstractions;

/// <summary>
/// Parses string tokens into numeric values of a type determined at runtime.
/// </summary>
public interface INumericParser
{
    /// <summary>
    /// Parses <paramref name="token"/> into a value of the specified numeric type.
    /// </summary>
    /// <param name="numericType">The numeric type to parse the token into. Must implement <see cref="System.Numerics.INumber{TSelf}"/>.</param>
    /// <param name="token">The string representation of the value to parse.</param>
    /// <param name="provider">An optional format provider used to interpret <paramref name="token"/>.</param>
    /// <returns>The parsed value, boxed as <see cref="object"/>.</returns>
    object Parse(Type numericType, string token, IFormatProvider? provider = null);
}