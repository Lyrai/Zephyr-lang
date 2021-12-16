using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public abstract class Node
    {
        public Token Token { get; protected init; }
        public object Value { get; protected set; }

        public virtual List<Node> GetChildren()
        {
            return new() {this};
        }
        public abstract T Accept<T>(INodeVisitor<T> visitor);

        public virtual void Replace(Node oldItem, Node newItem)
        { }
    }
}