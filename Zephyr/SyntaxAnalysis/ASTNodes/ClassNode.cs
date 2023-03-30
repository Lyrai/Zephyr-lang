using System.Collections.Generic;
using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class ClassNode : Node, IDeclaration
    {
        public List<Node> Body { get; }
        public string Name { get; }
        public ClassSymbol Symbol { get; set; }
        public VarNode Parent { get; }
        public override bool IsUsed => true;

        public ClassNode(Token token, VarNode parent, List<Node> body)
        {
            Token = token;
            Name = (string)token.Value;
            Body = body;
            Parent = parent;
        }
        
        public override List<Node> GetChildren()
        {
            return Body;
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitClassNode(this);
        }
    }
}
