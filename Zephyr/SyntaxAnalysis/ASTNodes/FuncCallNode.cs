using System;
using System.Collections.Generic;
using Zephyr.Interpreting;
using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class FuncCallNode : Node, IExpression
    {
        public List<Node> Arguments { get; }
        public string Name { get; }
        public ICallable Callable { get; set; }
        public Node Callee { get; set; }
        public bool IsStatement { get; private set; } = false;
        public bool IsUsed { get; private set; } = true;
        public bool ReturnsValue => TypeSymbol.Name != "void";
        public bool CanBeDropped => false;
        public TypeSymbol ReturnType => TypeSymbol;

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
