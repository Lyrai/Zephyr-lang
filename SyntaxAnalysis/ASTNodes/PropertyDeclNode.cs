using System;
using System.Collections.Generic;

namespace Zephyr.SyntaxAnalysis.ASTNodes
{
    public class PropertyDeclNode : Node
    {
        public string Name { get; }
        public Node Getter { get; }
        public Node Setter { get; }
        public Node Variable { get; }

        public PropertyDeclNode(Node variable, Node getter, Node setter)
        {
            Getter = getter;
            Setter = setter;
            Variable = variable;
            Token = variable.Token;
            Name = (string)Token.Value;
        }

        public override List<Node> GetChildren()
        {
            throw new NotSupportedException();
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
            return visitor.VisitPropertyDeclNode(this);
        }
    }
}