namespace Zephyr.SemanticAnalysis.Symbols
{
    public class VarSymbol : Symbol
    {
        public VarSymbol(string name, TypeSymbol type) : base(name, type)
        { }
    }
}