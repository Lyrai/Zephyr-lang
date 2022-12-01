using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SemanticAnalysis.SemanticNodes
{
    public class VarSemanticNode: SemanticNode
    {
        public VarSemanticNode(TypeSymbol type, string name) : base(type, name)
        { }

        public override T Accept<T>(ISemanticNodeVisitor<T> visitor)
        {
            return visitor.VisitVarNode(this);
        }
    }
}