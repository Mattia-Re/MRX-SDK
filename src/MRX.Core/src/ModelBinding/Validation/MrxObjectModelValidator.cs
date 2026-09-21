using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using MRX.Core.ModelBinding.Attributes;
using JsonPathMatching = MRX.Json.Path.JsonPathMatching;

namespace MRX.Core.ModelBinding.Validation;

/// <summary>
///     Provides a validation visitor that does not perform top-level model validation.
/// </summary>
/// <remarks>
///     The returned validation visitor, will cause the <see cref="RequiredAttribute" /> error message not to be
///     added to the ModelState.
/// </remarks>
internal class MrxObjectModelValidator(
    IModelMetadataProvider modelMetadataProvider,
    IOptions<MvcOptions> mvcOptions,
    IOptions<ModelBindingOptions> bindingOptions)
    : ObjectModelValidator(modelMetadataProvider, mvcOptions.Value.ModelValidatorProviders)
{
    private const string SeenKeysKey = "Mrx.ValidationFormatter.SeenKeys";
    private const string ValidatedParametersCounterKey = "Mrx.ValidationFormatter.ParameterCounter";
    private readonly ModelBindingOptions _bindingOptions = bindingOptions.Value;

    private readonly MvcOptions _mvcOptions = mvcOptions.Value;

    public override void Validate(ActionContext actionContext, ValidationStateDictionary? validationState,
        string? prefix, object? model,
        ModelMetadata metadata, object? container)
    {
        HttpContext httpContext = actionContext.HttpContext;
        ModelStateDictionary modelState = actionContext.ModelState;
        int validatedParametersCounter = (int)(httpContext.Items[ValidatedParametersCounterKey] ??= 0);

        // Get the binding source so that we can disambiguate colliding keys by appending it as prefix
        BindingSource? bindingSource = metadata.BindingSource ?? actionContext.ActionDescriptor.Parameters
            .FirstOrDefault(p => p.Name == metadata.ParameterName)
            ?.BindingInfo?.BindingSource;

        // We make prefixing with binding source opt-in by adding DisambiguateAttribute to the action or controller.
        bool shouldDisambiguate = actionContext.ActionDescriptor.EndpointMetadata.OfType<DisambiguateAttribute>().Any();

        // It is useful to signal the client whether error keys are prefixed with their respective binding source.
        // We add a header for this
        actionContext.HttpContext.Response.Headers.TryAdd(
            "X-Disambiguated",
            new StringValues(shouldDisambiguate.ToString()?.ToLowerInvariant()));

        // Keep track of keys we've already transformed. Either get the list of keys set by the previous
        // call or create an empty one
        HashSet<string> prevKeys =
            (HashSet<string>)(httpContext.Items[SeenKeysKey] ??= new HashSet<string>());

        base.Validate(actionContext, validationState, prefix, model, metadata, container);

        // Use diffing to extract keys we haven't processed yet.
        HashSet<string> newKeys = [.. modelState.Keys.Where(k => !prevKeys.Contains(k))];

        // Re-key and reformat
        foreach (string key in newKeys)
        {
            if (modelState.TryGetValue(key, out ModelStateEntry? value))
            {
                // Grab a copy of errors for this entry before removing it from the ModelState
                List<ModelError> valueErrors = [.. value.Errors];

                modelState.Remove(key);

                // Resolve the appropriate prefix based on the binding source of the parameter that is
                // being validated or fallback to root ($)
                string? bindingSourcePrefix = shouldDisambiguate
                    ? $"{bindingSource?.DisplayName ?? "$"}."
                    : null;

                // Per ADR-001 APIs must return consistent casing in error keys. We use a casing function
                // to apply the same casing to every JSON path token
                string formattedKey = CaseKey($"{bindingSourcePrefix}{key.TrimStart("$.")}");

                foreach (ModelError err in valueErrors)
                {
                    // We prefix the key with the count of parameters validated so that we can spot keys as new when
                    // disambiguation is disabled even if they were already processed. This is because the same
                    // binding source cannot contain colliding keys, so the same key appearing with a different counter
                    // state must belong to a different binding source than the previously processed one.
                    string stateKey = !shouldDisambiguate
                        ? $"{validatedParametersCounter}^{formattedKey}"
                        : formattedKey;

                    if (err.Exception != null)
                        modelState.AddModelError(stateKey, err.Exception, metadata);
                    else
                        modelState.AddModelError(stateKey, err.ErrorMessage);
                }
            }
        }

        // We store the keys we just processed in HttpContext.Items so that we can skip them in the next call
        foreach (string key in modelState.Keys)
            prevKeys.Add(key);

        // Increment the counter so that we can differentiate keys by parameter index
        httpContext.Items[ValidatedParametersCounterKey] = ++validatedParametersCounter;

        // Reformatting pollutes the model state with counter-prefixed keys when disambiguation is disabled.
        // Here we strip the counter prefix and merge colliding keys into a single entry
        if (validatedParametersCounter == actionContext.ActionDescriptor.Parameters.Count && !shouldDisambiguate)
        {
            KeyValuePair<string, ModelStateEntry>[] entries = [.. modelState];
            modelState.Clear();

            foreach ((string key, ModelStateEntry entry) in entries)
            {
                ModelError[] errors = [.. entry.Errors];
                string regKey = key.Split('^')[1];

                foreach (ModelError error in errors)
                {
                    if (error.Exception == null)
                        modelState.TryAddModelError(regKey, error.ErrorMessage);
                    else
                        modelState.TryAddModelError(regKey, error.Exception, metadata);
                }
            }
        }
    }

    private string CaseKey(string path)
    {
        return JsonPathMatching.JsonPathKeys()
            .Replace(path, m => _bindingOptions.ErrorKeyNamingPolicy.ConvertName(m.Value));
    }

    public override ValidationVisitor GetValidationVisitor(ActionContext actionContext,
        IModelValidatorProvider validatorProvider,
        ValidatorCache validatorCache, IModelMetadataProvider metadataProvider,
        ValidationStateDictionary? validationState)
    {
        // By returning a visitor that does not validate at top-level, no error for an unbound parameter will be
        // added to the ModelState. This is especially useful when the body parameter fails to bind due to
        // invalid JSON in the request body.
        return new RootRequiredSuppressingValidationVisitor(actionContext, validatorProvider, validatorCache,
            metadataProvider, validationState)
        {
            MaxValidationDepth = _mvcOptions.MaxValidationDepth,
            ValidateComplexTypesIfChildValidationFails = _mvcOptions.ValidateComplexTypesIfChildValidationFails
        };
    }
}