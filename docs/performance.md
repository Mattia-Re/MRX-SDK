# Performance

When [`MRX.UI.ErrorsAutoWiring`](xref:MRX.UI.ErrorsAutoWiring.Http.ProblemDetailsHttpClient) walks
a server-provided error key back onto your bound models, resolving an array-index token
(`[0]`, `[1]`, …) costs meaningfully different amounts of work depending on how the collection
property is typed. See [Model Node Accessors](concepts/model-node-accessors.md) for how these
accessors are selected; this page is about which container shapes to prefer for your bound models.

## Avoid custom indexed containers

If a collection property's type doesn't implement `IList` or `IEnumerable` but does expose a
numeric indexer (e.g. a custom `T this[int index]`), array-index tokens fall through to the
[`IModelNodeAccessor`](xref:MRX.Json.Reflection.Abstractions.IModelNodeAccessor) that resolves
custom indexers, which does so entirely through reflection:

- It calls `GetType().GetProperties()` and scans *every* public property of the container's type,
  on every single token resolution, to find the one declaring a single-parameter numeric indexer —
  there is no caching between calls.
- The index value is parsed and boxed to match the indexer's parameter type, and the indexer is then
  invoked via `PropertyInfo.GetValue`, which is substantially slower than a direct array or list
  access.

None of this is cached across calls, so the cost is paid again for every array-index token in every
key that needs resolving. Prefer a collection type that implements `IList` or `IEnumerable` instead
of relying on a custom indexer, unless you have no other option.

## Prefer `IList` over plain `IEnumerable`

Between `IList` and plain `IEnumerable`, prefer `IList` (e.g. `List<T>`, arrays, or any other
`IList`-implementing collection) for bound collection properties:

- Against an `IList`, the
  [`IModelNodeAccessor`](xref:MRX.Json.Reflection.Abstractions.IModelNodeAccessor) that handles
  array indices resolves it directly (`list[index]`) — an **O(1)** operation with no additional
  allocation.
- Against a plain `IEnumerable` that isn't also an `IList`, the same accessor falls back to
  `enumerable.Cast<object>().ElementAtOrDefault(index)`, which is **O(n)** in the requested index —
  it has to enumerate from the start every time — and allocates an enumerator (and boxes each value
  type element) to do it.

So, in order of preference: an `IList`-implementing collection first, a plain `IEnumerable` second,
and a type relying on a custom numeric indexer only as a last resort.
