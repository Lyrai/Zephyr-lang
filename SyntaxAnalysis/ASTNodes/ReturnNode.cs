using System;
using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class ReturnNode : Node
    {
        public Node Value { get; }
        
        public ReturnNode(Token token, Node value)
        {
            Token = token;
            Value = value;
        }

        public override List<Node> GetChildren()
        {
            return new() {Value};
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitReturnNode(this);
        }
    }
}