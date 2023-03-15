using System;
using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class UnOpNode : Node
    {
        public Node Operand { get; private set; }
        
        public UnOpNode(Token token, Node operand)
        {
            Token = token;
            Operand = operand;
        }

        public override List<Node> GetChildren()
        {
            return new() {Operand};
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitUnOpNode(this);
        }

        public override void Replace(Node oldItem, Node newItem)
        {
            if (oldItem != Operand)
                throw new ArgumentException("Incorrect item to replace");

            Operand = newItem;
        }
    }
}