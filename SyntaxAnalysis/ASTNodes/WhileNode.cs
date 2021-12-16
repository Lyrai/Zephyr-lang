namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class WhileNode : Node
    {
        public Node Condition { get; }
        public Node Body { get; }

        public WhileNode(Token token, Node condition, Node body)
        {
            Token = token;
            Condition = condition;
            Body = body;
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitWhileNode(this);
        }
    }
}