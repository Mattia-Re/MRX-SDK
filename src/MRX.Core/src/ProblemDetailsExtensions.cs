using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MRX.Core;

/// <summary>
///     Provides extension methods for reading strongly-typed values out of <see cref="ProblemDetails" /> extensions.
/// </summary>
public static class ProblemDetailsExtensions
{
    extension(ProblemDetails problemDetails)
    {
        /// <summary>
        ///     Attempts to retrieve and deserialize a value from <see cref="ProblemDetails.Extensions" /> by
        ///     <paramref name="key" />.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the extension value into.</typeparam>
        /// <param name="key">The extension key to look up.</param>
        /// <param name="value">
        ///     When this method returns, contains the deserialized value if <paramref name="key" /> was found and the raw
        ///     value was a <see cref="JsonElement" />; otherwise, the default value for <typeparamref name="TValue" />.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="key" /> was found; otherwise, <see langword="false" />.</returns>
        /// <exception cref="InvalidOperationException">The extension value was found but was not a <see cref="JsonElement" />.</exception>
        public bool TryGetValue<TValue>(string key, out TValue? value)
        {
            if (!problemDetails.Extensions.TryGetValue(key, out object? rawValue))
            {
                value = default;
                return false;
            }

            if (rawValue is not JsonElement jsonElement)
                throw new InvalidOperationException($"Raw value was not of type {typeof(JsonElement).FullName}.");

            value = jsonElement.Deserialize<TValue>();
            return true;
        }
    }
}