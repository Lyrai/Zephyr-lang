using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SemanticAnalysis.SemanticNodes
{
    public abstract class SemanticNode
    {
        public TypeSymbol Type { get; protected set; }
        public string? Name { get; protected set; }

        public SemanticNode(TypeSymbol type, string? name = null)
        {
            Type = type;
            Name = name;
        }

        public abstract T Accept<T>(ISemanticNodeVisitor<T> visitor);
    }
}