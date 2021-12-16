using System;
using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class NoOpNode : Node
    {
        public NoOpNode()
        { }
        public override List<Node> GetChildren()
        {
            throw new ArgumentException();
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitNoOpNode(this);
        }
    }
}