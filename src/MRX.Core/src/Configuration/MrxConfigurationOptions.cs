using MRX.Core.ModelBinding;

namespace MRX.Core.Configuration;

public class MrxConfigurationOptions
{
    public Action<ModelBindingOptions> ModelBindingOptions { get; set; } = _ => { };
}