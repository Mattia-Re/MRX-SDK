using Microsoft.AspNetCore.Components.Forms;
using MRX.Core.ModelBinding;
using MRX.UI.ErrorsAutoWiring.Abstractions;
using MRX.UI.ErrorsAutoWiring.Http;

namespace MRX.UI.ErrorsAutoWiring.Wiring;

public class DefaultErrorMappingFactory(IModelKeyPathVisitor modelVisitor) : IErrorMappingFactory
{
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

    private static string ResolveKeyPattern(string key, string? sourceName)
    {
        return sourceName != null
            ? $"{sourceName}.{key}"
            : key;
    }
}