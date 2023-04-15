using System.Collections.Generic;
using Zephyr.SemanticAnalysis;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class IfNode : Node, IExpression
    {
        public Node Condition { get; }
        public Node ThenBlock { get; private set; }
        public Node ElseBlock { get; private set; }
        public bool IsStatement { get; private set; }
        public bool IsUsed /*{ get; private set; }*/ => true;
        public bool ReturnsValue => ThenBlock is IExpression { ReturnsValue: true };

        public bool CanBeDropped => ThenBlock is IExpression { CanBeDropped: true } &&
                                    ElseBlock is IExpression { CanBeDropped: true };

        public IfNode(Token token, Node condition, Node thenBlock, Node elseBlock)
        {
            Token = token;
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }

        public override List<Node> GetChildren()
        {
            var children = new List<Node> { ThenBlock };
            if(ElseBlock is not null)
                children.Add(ElseBlock);

            return children;
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitIfNode(this);
        }

        public void SetIsStatement(bool isStatement)
        {
            IsStatement = isStatement;
        }

        public void SetUsed(bool used)
        {
            if (ThenBlock is IExpression thenBlock)
            {
                thenBlock.SetUsed(used);
            }

            if (ElseBlock is IExpression elseBlock)
            {
                elseBlock.SetUsed(used);
            }
        }

        public override void Replace(Node oldItem, Node newItem)
        {
            if (oldItem == ThenBlock)
            {
                ThenBlock = newItem;
            }
            else if (oldItem == ElseBlock)
            {
                ElseBlock = newItem;
            }
            else
            {
                throw new ArgumentException($"{nameof(oldItem)} is not contained in node");
            }
        }
    }
}
