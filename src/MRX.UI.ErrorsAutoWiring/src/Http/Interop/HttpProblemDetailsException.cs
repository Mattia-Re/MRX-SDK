using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MRX.UI.ErrorsAutoWiring.Http.Interop;

/// <summary>
/// Represents an RFC 7807 problem details response received from an HTTP call.
/// </summary>
public class HttpProblemDetailsException : Exception
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// A URI reference identifying the problem type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The HTTP status code of the response.
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// A short, human-readable summary of the problem type.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// A URI reference identifying the specific occurrence of the problem.
    /// </summary>
    public string? Instance { get; init; }

    /// <summary>
    /// Any additional members of the problem details response that are not mapped to a named property.
    /// </summary>
    [JsonExtensionData] public IDictionary<string, JsonElement>? Extensions { get; init; }

    /// <summary>
    /// Attempts to retrieve and deserialize an additional member from <see cref="Extensions"/>, matching its key
    /// case-insensitively.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the extension value as.</typeparam>
    /// <param name="extension">The extension key to look up.</param>
    /// <param name="value">When this method returns, the deserialized extension value, if found and successfully deserialized.</param>
    /// <returns><see langword="true"/> if the extension was found and successfully deserialized; otherwise, <see langword="false"/>.</returns>
    public bool TryGetExtension<TValue>(string extension, [NotNullWhen(true)] out TValue? value)
    {
        // Search for the key case-insensitive
        if (Extensions?.SingleOrDefault(kvp => kvp.Key.Equals(extension, StringComparison.OrdinalIgnoreCase))
                .Value is not { } ext)
        {
            value = default;
            return false;
        }

        TValue? data = ext.Deserialize<TValue>(_jsonOptions);
        if (data == null)
        {
            value = default;
            return false;
        }

        value = data;
        return true;
    }
}