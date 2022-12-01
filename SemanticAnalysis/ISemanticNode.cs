using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SemanticAnalysis
{
    public interface ISemanticNode
    {
        TypeSymbol TypeSymbol { get; }
        void SetType(TypeSymbol type);
    }
}