using System;

namespace Zephyr.SemanticAnalysis.Symbols
{
    public class VarSymbol : Symbol
    {
        public VarSymbol(string name, TypeSymbol type) : base(name, type)
        { }

        public void SetType(TypeSymbol type)
        {
            if(type.Name == "void")
                throw new ArgumentException("Variable cannot be of type void");

            Type = type;
        }
    }
}