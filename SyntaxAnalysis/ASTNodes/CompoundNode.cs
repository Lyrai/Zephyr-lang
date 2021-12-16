using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class CompoundNode : Node
    {
        private readonly List<Node> _children;
        
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
    }
}