using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.SemanticAnalysis.Symbols
{
    public class PropertySymbol : VarSymbol
    {
        public bool HasGetter { get; }
        public bool HasSetter { get; }

        public PropertySymbol(PropertyDeclNode node, TypeSymbol type) : base(node.Name, type)
        {
            HasGetter = node.Getter is not null;
            HasSetter = node.Setter is not null;
        }
    }
}