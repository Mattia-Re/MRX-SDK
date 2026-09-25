using Microsoft.AspNetCore.Components.Forms;
using MRX.Core.Common.ModelBinding;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.Http;

namespace MRX.UI.ErrorsAutoWiring.Wiring;

/// <summary>
/// Default <see cref="IErrorMappingFactory"/> implementation, which maps coded errors onto model fields using
/// the "X-Disambiguated" response header to determine whether error keys are prefixed with their binding source.
/// </summary>
/// <param name="modelVisitor">The visitor used to resolve key paths against binding source models.</param>
public class DefaultErrorMappingFactory(IModelKeyPathVisitor modelVisitor) : IErrorMappingFactory
{
    /// <summary>
    /// Builds a map of field identifiers to their associated coded errors.
    /// </summary>
    /// <param name="editContext">The edit context whose model must be a <see cref="DataBindingSources"/> instance.</param>
    /// <param name="errors">The coded errors, keyed by the server-provided error key.</param>
    /// <param name="response">The HTTP response the errors were extracted from; must include a valid "X-Disambiguated" header.</param>
    /// <returns>A dictionary linking each resolved field identifier to the list of errors that apply to it.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="editContext"/>'s model is not a <see cref="DataBindingSources"/>, or the "X-Disambiguated" header is missing or invalid.</exception>
    public Dictionary<FieldIdentifier, List<CodedError>> CreateErrorFieldMap(EditContext editContext,
        Dictionary<string, CodedError[]> errors, HttpResponseMessage response)
    {
        if (editContext.Model is not DataBindingSources sources)
            throw new InvalidOperationException(
                $"{nameof(DefaultErrorMappingFactory)} cannot run on an EditContext whose Model is not {typeof(DataBindingSources).FullName}");

        bool disambiguated = GetDisambiguatedHeaderValueOrThrow(response);

        Dictionary<FieldIdentifier, List<CodedError>> map = [];

        if (disambiguated)
        {
            // Group keys by source and strip trim from start by Length(key + dot)
            Dictionary<string, string[]> sourceKeys = errors.Keys
                .Where(k => k != "$")
                .GroupBy(k => k.Split('.')[0])
                .ToDictionary(
                    g => g.Key.ToLowerInvariant(),
                    g => g.Select(k => k[(g.Key.Length + 1)..]).ToArray());

            foreach ((string sourceName, string[] keys) in sourceKeys)
            {
                // We select the matching source object if the prefix points to a supported type
                object? container = sourceName switch
                {
                    "body" => sources.Body,
                    "query" => sources.Query,
                    _ => null
                };

                if (container == null) continue;
                MapAndAdd(container, keys, sourceName, errors, map);
            }
        }
        else
        {
            // Here we must scan all sources for all keys because we have no way of telling what source keys belong to
            foreach (object? source in sources)
            {
                if (source == null) continue;
                MapAndAdd(source, errors.Keys, null, errors, map);
            }
        }

        return map;
    }

    /// <summary>
    /// Reads and parses the "X-Disambiguated" header from <paramref name="response"/>.
    /// </summary>
    /// <param name="response">The HTTP response to read the header from.</param>
    /// <returns>The parsed boolean value of the header.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the header is missing or does not hold a single boolean value.</exception>
    private bool GetDisambiguatedHeaderValueOrThrow(HttpResponseMessage response)
    {
        // The header must be present in the HTTP response
        if (response.Headers.TryGetValues("X-Disambiguated", out IEnumerable<string>? valuesEnumerable))
        {
            // It must hold exactly one value of type bool
            string[] values = [.. valuesEnumerable];
            if (values.Length == 1 && bool.TryParse(values[0], out bool disambiguated))
            {
                return disambiguated;
            }
        }

        // If control reached this point, the header was either not set or invalid, thus we should throw
        throw new InvalidOperationException(
            $"{nameof(DefaultErrorMappingFactory)} requires the X-Disambiguated header to be set in HTTP responses and hold a single boolean value");
    }

    /// <summary>
    /// Resolves <paramref name="keys"/> against a single binding source <paramref name="container"/> and re-keys
    /// the matching errors by the resulting field identifiers.
    /// </summary>
    /// <param name="container">The binding source object to resolve the keys against.</param>
    /// <param name="keys">The error keys (without source prefix) to resolve.</param>
    /// <param name="sourceName">The binding source name used to reconstruct the original error key, or <see langword="null"/> when disambiguation is disabled.</param>
    /// <param name="errors">The full set of coded errors, keyed by the server-provided error key.</param>
    /// <returns>A dictionary linking each resolved field identifier to the list of errors that apply to it.</returns>
    private Dictionary<FieldIdentifier, List<CodedError>> VisitModel(object container,
        IEnumerable<string> keys, string? sourceName, Dictionary<string, CodedError[]> errors)
    {
        // We re-key the result of the model visitor so that:
        // - the key is the FieldIdentifier associated to the visited key path
        // - the value is the array of errors associated to the original key
        //
        // To find the matching errors array, we search for it in the errors dictionary by the original key.
        // The original key is resolved so that a single method can work both when disambiguation is enabled (in this
        // case the binding source prefix is stripped, so we add it back) and when it isn't (just the key returned by
        // the visitor)

        Dictionary<string, FieldIdentifier> keyField = modelVisitor.VisitModel(container.GetType(), container, keys);
        Dictionary<FieldIdentifier, List<CodedError>> mapPiece = [];

        foreach ((string key, FieldIdentifier fi) in keyField)
        {
            List<CodedError> keyErrors =
            [
                .. errors.Single(e =>
                    e.Key.Equals(ResolveKeyPattern(key, sourceName), StringComparison.OrdinalIgnoreCase)).Value
            ];

            if (!mapPiece.TryAdd(fi, keyErrors))
            {
                mapPiece[fi].AddRange(keyErrors);
            }
        }

        return mapPiece;
    }

    /// <summary>
    /// Resolves the given keys against a binding source container and merges the resulting field-to-errors mapping
    /// into <paramref name="map"/>.
    /// </summary>
    /// <param name="container">The binding source object to resolve the keys against.</param>
    /// <param name="keys">The error keys (without source prefix) to resolve.</param>
    /// <param name="sourceName">The binding source name used to reconstruct the original error key, or <see langword="null"/> when disambiguation is disabled.</param>
    /// <param name="errors">The full set of coded errors, keyed by the server-provided error key.</param>
    /// <param name="map">The map to merge the resolved field-to-errors mapping into.</param>
    private void MapAndAdd(object container,
        IEnumerable<string> keys, string? sourceName, Dictionary<string, CodedError[]> errors,
        Dictionary<FieldIdentifier, List<CodedError>> map)
    {
        // We collect all field-to-errors mappings for these keys and add them to the global map
        Dictionary<FieldIdentifier, List<CodedError>> piece = VisitModel(container, keys, sourceName, errors);
        foreach ((FieldIdentifier key, List<CodedError> keyErrors) in piece)
        {
            if (!map.TryAdd(key, keyErrors))
            {
                map[key].AddRange(keyErrors);
            }
        }
    }

    /// <summary>
    /// Reconstructs the original server-provided error key from a resolved <paramref name="key"/> and its
    /// binding source name (<paramref name="sourceName"/>).
    /// </summary>
    /// <param name="key">The key without the source prefix.</param>
    /// <param name="sourceName">The binding source name to prefix the key with, or <see langword="null"/> to leave the key unprefixed.</param>
    /// <returns>The reconstructed error key.</returns>
    private static string ResolveKeyPattern(string key, string? sourceName)
    {
        return sourceName != null
            ? $"{sourceName}.{key}"
            : key;
    }
}