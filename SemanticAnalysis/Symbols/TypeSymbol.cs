namespace Zephyr.SemanticAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        public TypeSymbol(string name) : base(name)
        { }

        public override string ToString()
        {
            return Name;
        }

        public static bool operator ==(TypeSymbol t1, TypeSymbol t2)
        {
            return t1?.Name == t2?.Name;
        }

        public static bool operator !=(TypeSymbol t1, TypeSymbol t2)
        {
            return !(t1 == t2);
        }

        public static TypeSymbol FromObject(object symbol)
        {
            if (symbol is not VarSymbol varSymbol)
                return (TypeSymbol) symbol;

            return varSymbol.Type;
        }
    }
}