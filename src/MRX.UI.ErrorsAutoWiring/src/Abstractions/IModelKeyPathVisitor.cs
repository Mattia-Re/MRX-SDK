using Microsoft.AspNetCore.Components.Forms;

namespace MRX.UI.ErrorsAutoWiring.Abstractions;

public interface IModelKeyPathVisitor
{
    Dictionary<string, FieldIdentifier> VisitModel<T>(T model, IEnumerable<string> keys);
    Dictionary<string, FieldIdentifier> VisitModel(Type modelType, object model, IEnumerable<string> keys);
}