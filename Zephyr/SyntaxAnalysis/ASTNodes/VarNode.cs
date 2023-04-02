using System;
using System.Collections.Generic;
using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class VarNode : Node, IExpression
    {
        public bool IsStatement { get; private set; }
        public bool IsUsed { get; private set; }
        public bool ReturnsValue => true;
        public bool CanBeDropped => false;
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
