namespace MRX.Parsing.Reflection.Abstractions;

public interface INumericParser
{
    object Parse(Type numericType, string token, IFormatProvider? provider = null);
}