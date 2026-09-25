# Root-Level Errors

## What a root-level error is

Not every validation error can be attributed to a specific field. The validation visitor installed
by [`ConfigureMrxModelBindingValidation`](xref:MRX.Core.ModelBinding.ModelBindingConfigurationExtensions)
suppresses top-level `[Required]` validation for unbound body parameters, so an invalid or missing
request body doesn't add a spurious property-level error — but that doesn't mean an endpoint can
never need to report an error that isn't about any one property. An error can concern the request
as a whole, or something entirely outside of it, such as the current state of the resource being
acted on (e.g. "this order has already been shipped and can no longer be edited").

The special key `$` exists for exactly this: it's a key developers can use to attach an error to
the response when there is no more specific key in the request's binding sources to attach it to,
rather than something ASP.NET Core surfaces on its own.

## Why JSON path keys can't represent it

Every other error key produced by MRX is a [JSON Path](xref:MRX.Json.Path.JsonPathWalker) made up
of one or more **nodes** — property names or array indices, walked token by token down to a
**leaf**, the specific field the error applies to. That's exactly what lets an
[`IModelKeyPathVisitor`](xref:MRX.UI.ErrorsAutoWiring.Abstractions.IModelKeyPathVisitor) resolve a
key like `address.street` back to a concrete `FieldIdentifier` on the client: it walks `address`,
then `street`, resolving an accessor at each step.

`$` isn't a node or a leaf — it's the absence of a path. The tokenizer that
[`JsonPathWalker`](xref:MRX.Json.Path.JsonPathWalker) is built on only recognizes property names and
array-index syntax (see
[`JsonPathMatching.JsonPathTokens`](xref:MRX.Json.Path.JsonPathMatching.JsonPathTokens)); `$` matches
neither, so walking it yields **zero tokens**. With no token to resolve, there is no accessor and
therefore no `FieldIdentifier` a root-level error could ever be attached to — by construction, the
JSON path key format has no way to express "this error belongs to the model as a whole" the same
way it expresses "this error belongs to this specific property."

## How MRX surfaces root-level errors instead

Because a `$` key can't be resolved to a field, it's handled as a special case everywhere it's
processed, rather than flowing through the same node/leaf resolution as every other key:

- On the client, [`ProblemDetailsHttpClient`](xref:MRX.UI.ErrorsAutoWiring.Http.ProblemDetailsHttpClient)
  checks for a `$` entry in the response's errors **before** handing the rest off to the
  configured `IErrorMappingFactory`, and adds any errors found there directly to
  [`DataBindingSources.RootError`](xref:MRX.UI.ErrorsAutoWiring.Http.DataBindingSources) — a
  dedicated string property meant to hold "an error that is not assignable to any more specific
  parameter."
- [`DefaultErrorMappingFactory`](xref:MRX.UI.ErrorsAutoWiring.Wiring.DefaultErrorMappingFactory)
  explicitly excludes `$` when grouping keys by binding source while disambiguation is enabled,
  since it has no binding-source prefix to strip. When disambiguation is disabled, a `$` key would
  simply be skipped further down the pipeline: `ModelKeyPathVisitor` fails to produce a token for
  it and moves on, so it never resolves to a `FieldIdentifier` there either.

If you're rendering root-level errors in a Blazor form, bind to `DataBindingSources.RootError`
directly (e.g. with `FieldIdentifier.Create(() => sources.RootError)`) rather than expecting them to
appear through the same per-field `ValidationMessage` wiring used for everything else — see
[`ProblemDetailsHttpClient.SendContextAwareAsync`](xref:MRX.UI.ErrorsAutoWiring.Http.ProblemDetailsHttpClient)
for exactly how it's populated.

`$` isn't the only key that can fail to resolve to a field this way — see
[Model Node Accessors](model-node-accessors.md) for the more general case of a key being skipped
because one of its path tokens can't be resolved at all.
