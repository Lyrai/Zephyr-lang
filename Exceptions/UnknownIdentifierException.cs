using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr
{
    public class UnknownIdentifierException : SemanticException
    {
        public UnknownIdentifierException(Node node) : base(node, $"Unknown identifier {node.Token.Value}")
        { }
    }
}