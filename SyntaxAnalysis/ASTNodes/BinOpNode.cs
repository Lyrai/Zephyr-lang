using System;
using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class BinOpNode : Node
    {
        public Node Left { get; private set; }
        public Node Right { get; private set; }

        public BinOpNode(Token token, Node left = null, Node right = null)
        {
            Token = token;
            Left = left;
            Right = right;
        }

        public override List<Node> GetChildren() => new() {Left, Right};
        
        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitBinOpNode(this);
        }

        public override void Replace(Node oldItem, Node newItem)
        {
            if (oldItem == Left)
                Left = newItem;
            else if (oldItem == Right)
                Right = newItem;
            else
                throw new ArgumentException("Incorrect item to replace");
        }
    }
}