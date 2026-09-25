# Getting Started

This walkthrough wires up MRX-flavored validation errors on an ASP.NET Core API and consumes them
from a Blazor client.

## Server: configure model binding validation

Install the package:

```
dotnet add package MRX.Core
```

Then, in `Program.cs`, opt in to the MRX validation pipeline:

```csharp
builder.Services.ConfigureMrxModelBindingValidation();
```

This replaces the default `IObjectModelValidator` with one that re-keys validation errors as
camelCase JSON paths and returns them as
[`CodedError`](xref:MRX.Core.Common.ModelBinding.CodedError) instances under an `errors` extension
on the `ProblemDetails` response, instead of the default free-text `ModelState` errors.

You can customize the casing applied to error keys:

```csharp
builder.Services.ConfigureMrxModelBindingValidation(options =>
{
    options.ErrorKeyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});
```

If an endpoint accepts request data from more than one binding source and those sources can have
colliding field names (e.g. a route parameter and a body property both named `id`), read
[Disambiguation](concepts/disambiguation.md) before deciding whether to opt that endpoint in to key
disambiguation.

## Client: auto-wire errors into a Blazor form

Install the package:

```
dotnet add package MRX.UI.ErrorsAutoWiring
```

Register the services, optionally configuring the underlying `HttpClient` (e.g. its `BaseAddress`):

```csharp
builder.Services.AddErrorsAutoWiringServices(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
});
```

In your component, bind the `EditContext`'s model to a
[`DataBindingSources`](xref:MRX.UI.ErrorsAutoWiring.Http.DataBindingSources) instance holding the
models used for the request body and query string, then send requests through
[`ProblemDetailsHttpClient`](xref:MRX.UI.ErrorsAutoWiring.Http.ProblemDetailsHttpClient):

```csharp
DataBindingSources sources = new() { Body = myRequestBody, Query = myRequestQuery };
EditContext editContext = new(sources);

HttpResponseMessage response = await problemDetailsHttpClient.SendContextAwareAsync(
    editContext,
    new HttpRequestMessage(HttpMethod.Post, "/orders") { Content = JsonContent.Create(myRequestBody) },
    cancellationToken);
```

`SendContextAwareAsync` maps any `errors` extension in the response back onto `editContext`'s
`ValidationMessageStore`, matching each server-provided JSON path to the corresponding field in
`myRequestBody` or `myRequestQuery` — no manual key-to-field mapping required, and no changes
needed in your Razor markup: `<ValidationMessage For="..."/>` keeps working as-is.

Continue to [Concepts](concepts/disambiguation.md) for a deeper look at how error keys are
resolved.
