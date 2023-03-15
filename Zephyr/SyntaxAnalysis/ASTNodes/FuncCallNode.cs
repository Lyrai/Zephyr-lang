using System;
using System.Collections.Generic;
using Zephyr.Interpreting;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class FuncCallNode : Node
    {
        public List<Node> Arguments { get; }
        public string Name { get; }
        public ICallable Callable { get; set; }
        public Node Callee { get; set; }

        public FuncCallNode(Node callee, Token token, List<Node> arguments)
        {
            Arguments = arguments;
            Name = (string)token.Value;
            Token = callee.Token;
            Callee = callee;
        }

        public override List<Node> GetChildren()
        {
            throw new NotSupportedException();
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitFuncCallNode(this);
        }
    }
}