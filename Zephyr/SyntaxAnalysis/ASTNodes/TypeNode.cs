using System;
using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class TypeNode : Node
    {
        public override bool IsUsed => true;

        public TypeNode(Token token)
        {
            Token = token;
        }
        
        public override List<Node> GetChildren()
        {
            throw new NotSupportedException();
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            throw new NotSupportedException();
        }
    }
}
