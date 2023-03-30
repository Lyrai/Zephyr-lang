using System;
using System.Collections.Generic;
using Zephyr.SemanticAnalysis;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class VarDeclNode : Node, IDeclaration
    {
        public VarNode Variable;
        public TypeNode Type;
        public override bool IsUsed => true;

        public VarDeclNode(Node variable, Node type)
        {
            Variable = (VarNode) variable;
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
