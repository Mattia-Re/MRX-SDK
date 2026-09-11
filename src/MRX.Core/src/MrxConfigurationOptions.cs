using MRX.Core.ModelBinding;

namespace MRX.Core;

public class MrxConfigurationOptions
{
    public Action<ModelBindingOptions> ModelBindingOptions { get; set; } = _ => { };
}