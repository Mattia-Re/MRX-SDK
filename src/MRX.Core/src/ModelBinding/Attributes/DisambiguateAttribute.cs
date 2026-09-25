using MRX.Core.Common.Features;
using MRX.Core.Features;

namespace MRX.Core.ModelBinding.Attributes;

/// <summary>
///     Marks a controller or action as opting in to prefixing validation error keys with their binding source, so
///     that colliding keys from different binding sources can be disambiguated.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisambiguateAttribute : Attribute, IFeatureDescriptor
{
    /// <inheritdoc />
    public string FeatureName => MrxEndpointFeatures.ErrorKeyDisambiguation;
}