using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MRX.Core.ModelBinding.Validation;

/// <summary>
///     Validates the supplied model without top-level validation.
/// </summary>
/// <remarks>
///     This validation visitor causes the <see cref="RequiredAttribute" /> error message not to be added to the
///     ModelState.
/// </remarks>
internal class RootRequiredSuppressingValidationVisitor(
    ActionContext actionContext,
    IModelValidatorProvider validatorProvider,
    ValidatorCache validatorCache,
    IModelMetadataProvider metadataProvider,
    ValidationStateDictionary? validationState)
    : ValidationVisitor(actionContext, validatorProvider, validatorCache, metadataProvider, validationState)
{
    public override bool Validate(ModelMetadata? metadata, string? key, object? model, bool alwaysValidateAtTopLevel,
        object? container)
    {
        // Ensure we only target null body models and let validation for things like query parameters flow normally

        if (alwaysValidateAtTopLevel && model == null && key != null && IsBodyParameter(key))
            // By not validating at top-level, no error for an unbound parameter will be
            // added to the ModelState. This is especially useful when the body parameter fails to bind due to
            // invalid JSON in the request body.
            alwaysValidateAtTopLevel = false;

        return base.Validate(metadata, key, model, alwaysValidateAtTopLevel, container);
    }

    /// <summary>
    ///     Returns whether the supplied key points to a parameter that binds to the body.
    /// </summary>
    /// <param name="key">Parameter name</param>
    private bool IsBodyParameter(string key)
    {
        ParameterDescriptor? parameter = Context.ActionDescriptor.Parameters.FirstOrDefault(p =>
            p.Name == key && p.BindingInfo?.BindingSource == BindingSource.Body);
        return parameter != null;
    }
}