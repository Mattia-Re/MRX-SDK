using System.Reflection;

namespace MRX.Json.Reflection.PropertyAccess;

internal record PropertyIndexerInfo(
    PropertyInfo PropertyInfo,
    Type IndexerType
);