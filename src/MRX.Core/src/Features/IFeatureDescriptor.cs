namespace MRX.Core.Features;

/// <summary>
///     Describes a named MRX endpoint feature, typically implemented by attributes applied to controllers or actions.
/// </summary>
public interface IFeatureDescriptor
{
    /// <summary>
    ///     The name of the feature this descriptor represents.
    /// </summary>
    public string FeatureName { get; }
}