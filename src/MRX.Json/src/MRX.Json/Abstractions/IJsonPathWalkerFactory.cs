namespace MRX.Json.Abstractions;

internal interface IJsonPathWalkerFactory
{
    IJsonPathWalker Create(string key);
}