namespace MRX.Json.Path;

public readonly record struct JsonPathToken(
    string Token,
    JsonPathTokenType Type,
    bool EndOfPath
);