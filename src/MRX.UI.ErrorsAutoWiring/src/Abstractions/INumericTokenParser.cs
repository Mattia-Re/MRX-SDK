namespace MRX.UI.ErrorsAutoWiring.Abstractions;

internal interface INumericTokenParser
{
    object Parse(Type indexerType, string token);
}