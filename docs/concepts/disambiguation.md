# Disambiguation

## What it is

By default, the `IObjectModelValidator` installed by
[`ConfigureMrxModelBindingValidation`](xref:MRX.Core.ModelBinding.ModelBindingConfigurationExtensions)
re-keys every validation error as a JSON path relative to the model it was raised on — for example
`id` for a property named `id`, regardless of whether that property came from the route, the query
string, or the request body. This is enough as long as no two binding sources on the same endpoint
expose a field with the same name.

When they do — say, an endpoint that binds an `id` route parameter *and* accepts a body with its
own `id` property — both validation errors would end up under the same key, `id`, with no way to
tell which source they came from.

**Disambiguation** solves this by prefixing every error key with the display name of the binding
source it came from (`Route.id`, `Body.id`, `Query.id`, …), making colliding keys distinguishable.

## How to enable it

Disambiguation is **opt-in**, per controller or per action, via
[`DisambiguateAttribute`](xref:MRX.Core.ModelBinding.Attributes.DisambiguateAttribute):

```csharp
[Disambiguate]
[HttpPut("{id}")]
public IActionResult UpdateOrder(int id, [FromBody] UpdateOrderRequest body) { ... }
```

Applying the attribute to a controller enables disambiguation for all of its actions; applying it
to an action enables it just for that action.

Whether disambiguation is active for a given request is also reported back to the client via the
`X-Disambiguated` response header (`"true"` or `"false"`), so that clients don't need to hardcode
per-endpoint assumptions — see [Feature Discovery](feature-discovery.md) for how this can be
discovered ahead of time as well.

## No Razor markup changes required

Disambiguation is entirely a server-side key-formatting concern. On the client,
[`MRX.UI.ErrorsAutoWiring`](xref:MRX.UI.ErrorsAutoWiring.Wiring.DefaultErrorMappingFactory) reads
the `X-Disambiguated` header itself: when it's `true`, it splits each error key on its first `.` to
recover the binding source and the remaining path, resolves that path against the matching
[`DataBindingSources`](xref:MRX.UI.ErrorsAutoWiring.Http.DataBindingSources) member (`Body` or
`Query`), and populates the `EditContext`'s validation messages exactly as it would without
disambiguation. Your components, bindings, and `<ValidationMessage For="..."/>` elements don't need
to know or care whether disambiguation is on for a given endpoint.

## The risk of leaving it disabled with colliding names

When disambiguation is **not** enabled, the client has no information telling it which binding
source an error key belongs to, so
[`DefaultErrorMappingFactory`](xref:MRX.UI.ErrorsAutoWiring.Wiring.DefaultErrorMappingFactory)
resolves every error key against *every* binding source model in turn (`Body`, then `Query`). If a
field name collides across sources — e.g. `id` exists on both the route-bound parameter and the
body model — the same error entry gets attached to *both* fields' `FieldIdentifier`, because the
client has no way to tell they aren't the same field.

In practice, this means: **if colliding field names exist across binding sources on an endpoint
that does not have `[Disambiguate]` applied, all merged errors for that key will be displayed next
to every field sharing that name**, not just the one the error actually applies to. This can
surface confusing or incorrect validation messages to the user.

To avoid this, apply `[Disambiguate]` to any controller or action where binding sources can
plausibly share field names. If you're unsure whether an endpoint's binding sources can collide, it
is always safe to enable disambiguation — it has no effect on the client-side developer experience
beyond correctly separating otherwise-ambiguous keys.
