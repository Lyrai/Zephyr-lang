using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SemanticAnalysis.SemanticNodes
{
    public class LiteralSemanticNode: SemanticNode
    {
        public LiteralSemanticNode(TypeSymbol type) : base(type, null)
        { }

        public override T Accept<T>(ISemanticNodeVisitor<T> visitor)
        {
            return visitor.VisitLiteralNode(this);
        }
    }
}