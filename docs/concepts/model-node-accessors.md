# Model Node Accessors

## Walking a JSON path onto a model

When an [`IModelKeyPathVisitor`](xref:MRX.UI.ErrorsAutoWiring.Abstractions.IModelKeyPathVisitor)
resolves a server error key such as `addresses[0].street` back to a field on your bound model, it doesn't reason
about the model's shape directly. Instead, it walks the key one
[JSON path token](xref:MRX.Json.Path.JsonPathWalker) at a time — `addresses`, `[0]`, `street` — and
at each step asks an [`IModelNodeAccessor`](xref:MRX.Json.Reflection.Abstractions.IModelNodeAccessor)
to resolve that token against the current object, moving the "current object" to whatever the
accessor returns before resolving the next token.

An `IModelNodeAccessor` answers two questions about a single token and container:

- `CanHandle` — can this accessor make sense of this token against this container at all?
- `GetValue` — given that it can, what value does the token refer to?

A container can also implement
[`IModelNodeNameAccessor`](xref:MRX.Json.Reflection.Abstractions.IModelNodeNameAccessor) to expose
the *canonical* member name a token maps to (e.g. resolving the casing-insensitive token `street` to
the actual `Street` property name) — this is what lets the final token in a path be turned into a
`FieldIdentifier` rather than just a value.

## The built-in accessors

[`AddMrxJsonReflection`](xref:MRX.Json.Reflection.DependencyInjection.JsonReflectionServiceCollectionExtensions)
registers three accessors, tried in registration order by
[`ModelPropertyAccessorProvider`](xref:MRX.Json.Reflection.Abstractions.IModelPropertyAccessorProvider):

1. **Property-name tokens**, resolved by case-insensitive reflection lookup against the container's
   public properties.
2. **Numeric array-index tokens against `IList`/`IEnumerable` containers**, resolved by indexing the
   list directly (or via `ElementAtOrDefault` for a plain `IEnumerable`), returning `null` for an
   out-of-range index rather than throwing.
3. **Numeric array-index tokens against a custom numeric indexer**, for container types that expose
   an indexer property with a single numeric parameter instead of implementing `IList`/`IEnumerable`.
   The token is parsed into the indexer's parameter type via `INumericParser` before invoking it.

The provider picks the *first* registered accessor whose `CanHandle` returns `true` for the given
token and container — it does not try every accessor and pick the best match, so registration order
matters when more than one accessor could plausibly handle the same token type.

## Extending it

Because accessors are registered as an `IEnumerable<IModelNodeAccessor>` via `TryAddEnumerable`, you
can register additional accessors alongside the built-in ones — for example, to support a custom
dictionary-like container type that isn't an `IList` and doesn't expose a numeric indexer. Register
your own `IModelNodeAccessor` implementation before or after calling `AddMrxJsonReflection`, in the
order you want it considered relative to the built-in accessors.

## What happens when no accessor can handle a token

If none of the registered accessors' `CanHandle` returns `true` for a given token — for instance,
the key refers to a property that doesn't exist on the client's model, or indexes into something
none of the registered accessors recognize — the walk simply stops at that token. No accessor is
found, so no value or field can be resolved from that point on, and the entire key is skipped:
no partial match is recorded, and no error is raised for the failure itself.

This means a key can silently fail to reach any field whenever the server and client models have
drifted out of sync, exactly as it does for [root-level errors that can't be resolved at
all](root-level-errors.md). Since the failure is silent by design, don't assume every error
returned by the server is guaranteed to surface next to a field — an error whose key can't be fully
walked is simply dropped from the `EditContext`'s validation messages rather than causing a fault.
