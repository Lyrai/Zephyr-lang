using System;
using System.Collections.Generic;
using Zephyr.SemanticAnalysis;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class LiteralNode : Node, IExpression
    {
        public bool IsStatement { get; private set; }
        public bool IsUsed { get; private set; }
        public bool ReturnsValue => true;
        public bool CanBeDropped => true;


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

        public void SetIsStatement(bool isStatement)
        {
            IsStatement = isStatement;
        }

        public void SetUsed(bool used)
        {
            IsUsed = used;
        }
    }
}
