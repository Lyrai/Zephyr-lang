using System;
using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class VarDeclNode : Node
    {
        public VarNode Variable;
        public TypeNode Type;

        public VarDeclNode(Node variable, Node type)
        {
            Variable = (VarNode)variable;
            Type = (TypeNode) type;
            Token = Variable.Token;
        }
        
        public override List<Node> GetChildren()
        {
            throw new NotSupportedException();
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitVarDeclNode(this);
        }
    }
}