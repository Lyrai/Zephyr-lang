using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class FuncDeclNode : Node, IDeclaration
    {
        public Node Body { get; private set; }
        public List<Node> Parameters { get; }
        public string Name { get; }
        public string ReturnType { get; }
        public FuncSymbol Symbol { get; set; }
        public bool IsStatic { get; private set; }

        public FuncDeclNode(Token token, Node body, List<Node> parameters, string returnType)
        {
            Body = body;
            Parameters = parameters;
            Name = (string)token.Value;
            ReturnType = returnType;
            Token = token;
        }

        public override List<Node> GetChildren()
        {
            throw new NotSupportedException();
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitFuncDeclNode(this);
        }

        public bool IsEmpty()
        {
            return Body is CompoundNode compound && (compound.GetChildren().Count == 0 ||
                                                     compound.GetChildren().Count == 1 &&
                                                     compound.GetChildren()[0] is NoOpNode) || Body is NoOpNode;
        }

        public void SetStatic(bool isStatic)
        {
            IsStatic = isStatic;
        }

        public override void Replace(Node oldItem, Node newItem)
        {
            if (oldItem != Body)
            {
                throw new ArgumentException($"{nameof(oldItem)} must be function body");
            }

            Body = newItem;
        }
    }
}
