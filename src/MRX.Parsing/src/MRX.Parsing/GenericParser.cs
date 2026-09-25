namespace MRX.Parsing;

/// <summary>
/// Provides generic parsing of strings into <see cref="IParsable{TSelf}"/> values.
/// </summary>
internal static class GenericParser
{
    /// <summary>
    /// Parses <paramref name="s"/> into a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The parsable type to parse the string into.</typeparam>
    /// <param name="s">The string representation of the value to parse.</param>
    /// <param name="provider">An optional format provider used to interpret <paramref name="s"/>.</param>
    /// <returns>The parsed value of type <typeparamref name="T"/>.</returns>
    internal static T ParseValue<T>(string s, IFormatProvider? provider = null) where T : IParsable<T>
        => T.Parse(s, provider);
}