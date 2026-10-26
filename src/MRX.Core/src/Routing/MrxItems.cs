namespace MRX.Core.Routing;

/// <summary>
///     Discards the <see cref="Microsoft.AspNetCore.Http.HttpContext.Items" /> entries MRX components keep for the
///     current execution of the request pipeline.
/// </summary>
internal static class MrxItems
{
    /// <summary>
    ///     The prefix of every <see cref="Microsoft.AspNetCore.Http.HttpContext.Items" /> key used by MRX.
    /// </summary>
    private const string KeyPrefix = "Mrx.";

    /// <summary>
    ///     Removes every entry whose key starts with <see cref="KeyPrefix" />.
    /// </summary>
    /// <param name="items">The <see cref="Microsoft.AspNetCore.Http.HttpContext.Items" /> to remove the entries from.</param>
    public static void Clear(IDictionary<object, object?> items)
    {
        foreach (object key in items.Keys.Where(k => k is string s && s.StartsWith(KeyPrefix, StringComparison.Ordinal))
                     .ToList())
            items.Remove(key);
    }
}
