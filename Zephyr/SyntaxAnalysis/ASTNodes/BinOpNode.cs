using System;
using System.Collections.Generic;
using Zephyr.SemanticAnalysis;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class BinOpNode : Node, IExpression
    {
        public bool IsStatement { get; private set; }
        public bool IsUsed => Token.Value.ToString() == "=" || _isUsed;
        public bool ReturnsValue => Token.Value.ToString() != "=";
        public bool CanBeDropped => Left is IExpression { CanBeDropped: true } &&
                                    Right is IExpression { CanBeDropped: true };
        
        public Node Left { get; private set; }
        public Node Right { get; private set; }

        private bool _isUsed;

        public BinOpNode(Token token, Node left = null, Node right = null)
        {
            Token = token;
            Left = left;
            Right = right;
        }

        public override List<Node> GetChildren() => new() {Left, Right};
        
        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitBinOpNode(this);
        }

        public override void Replace(Node oldItem, Node newItem)
        {
            if (oldItem == Left)
                Left = newItem;
            else if (oldItem == Right)
                Right = newItem;
            else
                throw new ArgumentException("Incorrect item to replace");
        }

        public void SetIsStatement(bool isStatement)
        {
            IsStatement = isStatement;
        }

        public void SetUsed(bool used)
        {
            _isUsed = used;
        }
    }
}
