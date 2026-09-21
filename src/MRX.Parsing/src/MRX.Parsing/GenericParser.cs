namespace MRX.Parsing;

internal static class GenericParser
{
    internal static T ParseValue<T>(string s, IFormatProvider? provider = null) where T : IParsable<T>
        => T.Parse(s, provider);
}