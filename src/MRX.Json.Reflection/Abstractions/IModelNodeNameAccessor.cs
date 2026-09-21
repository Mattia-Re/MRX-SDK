namespace MRX.Json.Reflection.Abstractions;

public interface IModelNodeNameAccessor
{
    string? GetNodeName(string token, object container);
}