using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr
{
    public class DuplicateIdentifierException : SemanticException
    {
        public DuplicateIdentifierException(Node node) : base(node, $"Redefenition of {(string)node.Token.Value}")
        { }
    }
}