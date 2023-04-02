using System;
using System.Collections.Generic;
using Zephyr.SemanticAnalysis;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class UnOpNode : Node, IExpression
    {
        public bool IsStatement { get; private set; }
        public bool IsUsed { get; private set; }
        public bool ReturnsValue => !IsPrint();
        public bool CanBeDropped => !IsPrint() && Operand is IExpression expr && expr.CanBeDropped;
        public Node Operand { get; private set; }
        
        public UnOpNode(Token token, Node operand)
        {
            Token = token;
            Operand = operand;
        }

        public override List<Node> GetChildren()
        {
            return new() {Operand};
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitUnOpNode(this);
        }

        public override void Replace(Node oldItem, Node newItem)
        {
            if (oldItem != Operand)
                throw new ArgumentException("Incorrect item to replace");

            Operand = newItem;
        }

        public void SetIsStatement(bool isStatement)
        {
            IsStatement = isStatement;
        }

        public void SetUsed(bool used)
        {
            IsUsed = used;
        }

        public bool IsPrint()
        {
            return Token.Value.ToString() == "print";
        }
    }
}
