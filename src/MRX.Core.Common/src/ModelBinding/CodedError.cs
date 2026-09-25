namespace MRX.Core.Common.ModelBinding;

/// <summary>
///     Represents a single validation error with a machine-readable code alongside its human-readable description.
/// </summary>
/// <param name="Code">The machine-readable error code, e.g. "InvalidValue".</param>
/// <param name="Description">The human-readable error message.</param>
public record CodedError(
    string Code,
    string Description
);