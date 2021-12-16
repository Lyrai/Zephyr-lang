using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr
{
    public class SemanticException : GeneralException
    {
        public SemanticException(Token token, string message) : base($"{message} at line {token.Line}")
        { }

        public SemanticException(Node node, string message) : base($"{message} at line {node.Token?.Line}")
        { }
    }
}