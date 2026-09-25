using System.Reflection;

namespace MRX.Json.Reflection.PropertyAccess;

/// <summary>
/// Describes a property that exposes a single-parameter numeric indexer.
/// </summary>
/// <param name="PropertyInfo">Reflection metadata for the indexer property.</param>
/// <param name="IndexerType">The type of the indexer's numeric parameter.</param>
internal record PropertyIndexerInfo(
    PropertyInfo PropertyInfo,
    Type IndexerType
);