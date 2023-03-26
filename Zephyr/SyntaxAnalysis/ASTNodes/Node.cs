using System.Collections.Generic;
using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public abstract class Node: ISemanticNode
    {
        public Token Token { get; protected init; }
        public object Value { get; protected set; }

        public TypeSymbol TypeSymbol => _type;
        
        public bool IsLhs { get; protected set; }
        
        private TypeSymbol _type;

        public virtual List<Node> GetChildren()
        {
            return new() {this};
        }
        public abstract T Accept<T>(INodeVisitor<T> visitor);

        public virtual void Replace(Node oldItem, Node newItem)
        { }
        
        public void SetType(TypeSymbol type)
        {
            _type = type;
        }

        public void SetLhs(bool lhs)
        {
            IsLhs = lhs;
        }
    }
}
