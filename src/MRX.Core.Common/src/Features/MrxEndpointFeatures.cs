namespace MRX.Core.Common.Features;

/// <summary>
///     Well-known feature names used to describe MRX endpoint behaviors via <c>IFeatureDescriptor</c>.
/// </summary>
public static class MrxEndpointFeatures
{
    /// <summary>
    ///     Identifies the feature that prefixes validation error keys with their binding source to disambiguate
    ///     colliding keys.
    /// </summary>
    public const string ErrorKeyDisambiguation = "ERROR_KEY_DISAMBIGUATION";
}