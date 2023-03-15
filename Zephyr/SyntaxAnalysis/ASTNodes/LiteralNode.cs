using System;
using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class LiteralNode : Node
    {
        public LiteralNode(Token token, object value)
        {
            Token = token;
            Value = value;
        }
        
        public override List<Node> GetChildren()
        {
            throw new NotSupportedException();
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitLiteralNode(this);
        }
    }
}