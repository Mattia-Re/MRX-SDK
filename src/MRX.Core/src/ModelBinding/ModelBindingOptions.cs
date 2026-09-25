using System.Text.Json;

namespace MRX.Core.ModelBinding;

/// <summary>
///     Options controlling how MRX formats model binding validation error keys.
/// </summary>
public class ModelBindingOptions
{
    /// <summary>
    ///     The naming policy applied to JSON path tokens when no <see cref="ErrorKeyNamingPolicy" /> is explicitly set.
    ///     Defaults to <see cref="JsonNamingPolicy.CamelCase" />.
    /// </summary>
    public JsonNamingPolicy DefaultNamingPolicy { get; set; } = JsonNamingPolicy.CamelCase;

    /// <summary>
    ///     The naming policy applied to validation error key JSON path tokens. Falls back to
    ///     <see cref="DefaultNamingPolicy" /> when not explicitly set.
    /// </summary>
    public JsonNamingPolicy ErrorKeyNamingPolicy
    {
        get => field ?? DefaultNamingPolicy;
        set;
    }
}