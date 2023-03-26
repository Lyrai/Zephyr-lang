using System;
using System.Collections.Generic;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class VarNode : Node
    {
        public object Symbol { get; set; }
        public string Name { get; }
        
        public VarNode(Token token, bool isLhs)
        {
            Token = token;
            Name = (string)token.Value;
            IsLhs = isLhs;
        }

        public override List<Node> GetChildren()
        {
            throw new NotSupportedException();
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitVarNode(this);
        }
    }
}
