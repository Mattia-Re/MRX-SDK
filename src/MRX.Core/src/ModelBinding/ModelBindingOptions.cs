using System.Text.Json;

namespace MRX.Core.ModelBinding;

public class ModelBindingOptions
{
    public JsonNamingPolicy DefaultNamingPolicy { get; set; } = JsonNamingPolicy.CamelCase;
    public JsonNamingPolicy ErrorKeyNamingPolicy { get => field ?? DefaultNamingPolicy; set; }
}