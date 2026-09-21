using System.Reflection;

namespace MRX.UI.ErrorsAutoWiring.PropertyAccess;

internal record PropertyIndexerInfo(
    PropertyInfo PropertyInfo,
    Type IndexerType
);