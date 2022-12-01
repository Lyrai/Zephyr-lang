using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SemanticAnalysis.SemanticNodes
{
    public class VarDeclSemanticNode: SemanticNode
    {
        public VarDeclSemanticNode(TypeSymbol type, string name) : base(type, name)
        { }

        public override T Accept<T>(ISemanticNodeVisitor<T> visitor)
        {
            return visitor.VisitVarDeclNode(this);
        }
    }
}