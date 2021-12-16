using System.Collections.Generic;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.Interpreting
{
    public interface ICallable
    {
        TypeSymbol ReturnType { get; }
        int Arity();
        object Call(Interpreter interpreter, List<object> arguments);
        bool TypesEqual(List<TypeSymbol> parameters);
    }
}