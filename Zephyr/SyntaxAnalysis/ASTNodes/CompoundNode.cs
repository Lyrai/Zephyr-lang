using System.Collections.Generic;
using Zephyr.SemanticAnalysis;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class CompoundNode : Node, IExpression
    {
        private readonly List<Node> _children;
        public bool IsStatement { get; private set; }
        public bool IsUsed { get; private set; }
        public bool ReturnsValue => _children.Last() is IExpression { ReturnsValue: true };
        public bool CanBeDropped => _children.All(child => child is IExpression expr && expr.CanBeDropped);

        public CompoundNode(List<Node> children)
        {
            _children = children;
        }

        public override List<Node> GetChildren()
        {
            return _children;
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitCompoundNode(this);
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
