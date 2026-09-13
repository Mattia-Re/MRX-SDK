using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MRX.Core.Common;

public static class ProblemDetailsExtensions
{
    extension(ProblemDetails problemDetails)
    {
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