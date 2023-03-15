using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class IfNode : Node
    {
        public Node Condition { get; }
        public Node ThenBlock { get; }
        public Node ElseBlock { get; }

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
    }
}