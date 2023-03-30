using System;
using System.Collections.Generic;
using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class FuncDeclNode : Node, IDeclaration
    {
        public List<Node> Body { get; }
        public List<Node> Parameters { get; }
        public string Name { get; }
        public string ReturnType { get; }
        public FuncSymbol Symbol { get; set; }
        public override bool IsUsed => true;

        public FuncDeclNode(Token token, List<Node> body, List<Node> parameters, string returnType)
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
            return Body.Count == 1 && Body[0] is NoOpNode || Body.Count == 0;
        }
    }
}
