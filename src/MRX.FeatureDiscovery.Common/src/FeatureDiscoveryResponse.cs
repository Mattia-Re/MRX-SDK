namespace MRX.FeatureDiscovery.Common;

/// <summary>
/// Response payload describing the MRX features enabled for a probed endpoint.
/// </summary>
/// <param name="Features">The names of the features enabled for the endpoint.</param>
public record FeatureDiscoveryResponse(IEnumerable<string> Features);