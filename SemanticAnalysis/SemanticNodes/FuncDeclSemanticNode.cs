using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SemanticAnalysis.SemanticNodes
{
    public class FuncDeclSemanticNode: SemanticNode
    {
        public SemanticNode Body { get; protected set; }
        
        public FuncDeclSemanticNode(TypeSymbol type, string name) : base(type, name)
        { }

        public override T Accept<T>(ISemanticNodeVisitor<T> visitor)
        {
            return visitor.VisitFuncDeclNode(this);
        }
    }
}