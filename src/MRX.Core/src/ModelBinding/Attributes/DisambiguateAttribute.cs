using MRX.Core.Common.Features;
using MRX.Core.Features;

namespace MRX.Core.ModelBinding.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisambiguateAttribute : Attribute, IFeatureDescriptor
{
    public string FeatureName => MrxEndpointFeatures.ErrorKeyDisambiguation;
}