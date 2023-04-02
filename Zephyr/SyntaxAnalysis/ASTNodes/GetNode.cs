using Zephyr.SemanticAnalysis;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class GetNode : Node//, IExpression
    {
        public Node Obj { get; }
        public bool IsStatement { get; private set; } = false;
        public bool IsUsed { get; private set; } = true;

        public GetNode(Token token, Node obj)
        {
            Token = token;
            Obj = obj;
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitGetNode(this);
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
