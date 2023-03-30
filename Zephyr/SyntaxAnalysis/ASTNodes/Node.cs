using System.Collections.Generic;
using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public abstract class Node: ISemanticNode, IStatement
    {
        public Token Token { get; protected init; }
        public object Value { get; protected set; }
        public virtual bool IsUsed => _isUsed;
        public TypeSymbol TypeSymbol => _type;
        public bool IsLhs { get; protected set; }
        
        private TypeSymbol _type;
        protected bool _isUsed;

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
        
        public virtual void SetUsed(bool used, bool recursive = true)
        {
            _isUsed = used;
            if (!recursive)
                return;

            foreach (var child in GetChildren())
            {
                child.SetUsed(used, recursive);
            }
        }
    }
}
