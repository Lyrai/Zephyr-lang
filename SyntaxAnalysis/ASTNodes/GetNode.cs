namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class GetNode : Node
    {
        public Node Obj { get; }

        public GetNode(Token token, Node obj)
        {
            Token = token;
            Obj = obj;
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitGetNode(this);
        }
    }
}