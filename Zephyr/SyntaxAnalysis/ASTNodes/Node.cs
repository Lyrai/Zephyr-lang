using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public abstract class Node: ISemanticNode
    {
        public Token Token { get; protected init; }
        public object Value { get; protected set; }
        public virtual TypeSymbol TypeSymbol { get; private set; }

        public bool IsLhs { get; protected set; }

        public virtual List<Node> GetChildren()
        {
            return null;
        }
        public abstract T Accept<T>(INodeVisitor<T> visitor);

        public virtual void Replace(Node oldItem, Node newItem)
        { }
        
        public void SetType(TypeSymbol type)
        {
            TypeSymbol = type;
        }

        public void SetLhs(bool lhs)
        {
            IsLhs = lhs;
        }
    }
}
