namespace MRX.Parsing.Reflection.Abstractions;

internal interface INumericParser
{
    object Parse(Type numericType, string token, IFormatProvider? provider = null);
}