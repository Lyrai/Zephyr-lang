using System;

namespace Zephyr.SemanticAnalysis.Symbols
{
    public abstract class Symbol
    {
        public string Name { get; init; }
        public TypeSymbol Type { get; init; }

        protected Symbol(string name, TypeSymbol type = null)
        {
            Name = name;
            Type = type;
        }

        protected Symbol()
        { }
    }
}