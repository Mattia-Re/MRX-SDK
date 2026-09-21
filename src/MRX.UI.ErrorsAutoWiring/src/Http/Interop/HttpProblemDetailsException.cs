using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MRX.UI.ErrorsAutoWiring.Http.Interop;

public class HttpProblemDetailsException : Exception
{
    public string Type { get; init; } = string.Empty;
    public int Status { get; init; }
    public string? Title { get; init; }
    public string? Detail { get; init; }
    public string? Instance { get; init; }

    [JsonExtensionData] public IDictionary<string, JsonElement>? Extensions { get; init; }

    public bool TryGetExtension<TValue>(string extension, [NotNullWhen(true)] out TValue? value)
    {
        // Search for the key case-insensitive
        if (Extensions?.SingleOrDefault(kvp => kvp.Key.Equals(extension, StringComparison.OrdinalIgnoreCase))
                .Value is not { } ext)
        {
            value = default;
            return false;
        }

        TValue? data = ext.Deserialize<TValue>();
        if (data == null)
        {
            value = default;
            return false;
        }

        value = data;
        return true;
    }
}