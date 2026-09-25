using MRX.Core.ModelBinding;

namespace MRX.Core.Configuration;

/// <summary>
///     Top-level configuration options for MRX services registered via <c>AddMrx</c>.
/// </summary>
public class MrxConfigurationOptions
{
    /// <summary>
    ///     A delegate used to configure the <see cref="MRX.Core.ModelBinding.ModelBindingOptions" /> applied to model
    ///     binding validation.
    /// </summary>
    public Action<ModelBindingOptions> ModelBindingOptions { get; set; } = _ => { };
}