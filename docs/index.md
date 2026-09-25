# Introduction

MRX SDK is a set of small, focused .NET libraries built around a single idea: an API and its
clients should agree on a well-structured, machine-readable error contract, and the plumbing that
keeps them in sync should be automatic rather than hand-written.

## The problem

ASP.NET Core's default model validation reports errors as flat, human-readable messages keyed by
model binding path (e.g. `"Address.Street"`). This is fine for logging, but it's awkward for
clients that want to:

- Map an error back to a specific form field reliably, including when the field's name collides
  across binding sources (route, query, body).
- Distinguish "the value at this JSON path is invalid" from "an unrelated exception occurred while
  binding".
- Read a machine-readable error code, not just a free-text message, so the UI can localize or
  branch on it.

## The pieces

- **`MRX.Core`** replaces the default ASP.NET Core `IObjectModelValidator` with one that re-keys
  validation errors into consistent, camelCase-by-default JSON paths and converts them into
  [`CodedError`](xref:MRX.Core.Common.ModelBinding.CodedError) instances — a `(Code, Description)`
  pair — under a stable `errors` extension on the `ProblemDetails` response. See
  [Disambiguation](concepts/disambiguation.md) for how it resolves colliding keys across binding
  sources.
- **`MRX.Core.Common`** holds the types shared between server and client (`CodedError`, well-known
  feature names), with no dependency on the ASP.NET Core shared framework, so it can be referenced
  from Blazor WebAssembly projects too.
- **`MRX.UI.ErrorsAutoWiring`** is the client-side counterpart: it reads the `errors` extension off
  an HTTP response and, using [`MRX.Json`](https://github.com/Mattia-Re/MRX-SDK) and
  [`MRX.Json.Reflection`](https://github.com/Mattia-Re/MRX-SDK) to walk each JSON path back onto
  your bound request models, populates a Blazor `EditContext`'s validation messages automatically —
  no manual mapping between error keys and form fields.
- **`MRX.FeatureDiscovery`** lets a client ask, for a given route and HTTP method, which MRX
  features (such as disambiguation) are active on that endpoint, so clients don't have to guess or
  hardcode assumptions about server behavior.
- **`MRX.Json`**, **`MRX.Json.Reflection`**, **`MRX.Parsing`**, and **`MRX.Parsing.Reflection`** are
  the lower-level building blocks (JSON Path parsing/walking, reflection-based property access and
  type conversion) that the higher-level packages are built on. They're usable standalone if you
  only need JSON Path matching or generic parsing.

### Limitations

Only body, query, URL path and header binding sources are currently supported.

Continue to [Getting Started](getting-started.md) to wire these up in a project.
