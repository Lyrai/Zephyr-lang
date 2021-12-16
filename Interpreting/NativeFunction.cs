using System;
using System.Collections.Generic;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.Interpreting
{
    public class NativeFunction : ICallable
    {
        private string _name;
        private Func<Interpreter, List<object>, object> _function;
        public TypeSymbol ReturnType { get; init; }
        public List<TypeSymbol> ParameterTypes { get; init; } = new();

        public NativeFunction(string name, Func<Interpreter, List<object>, object> function)
        {
            _name = name;
            _function = function;
        }

        public int Arity()
        {
            return ParameterTypes.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return _function(interpreter, arguments);
        }

        public bool TypesEqual(List<TypeSymbol> parameters)
        {
            if (parameters.Count != ParameterTypes.Count)
                return false;

            for (var i = 0; i < ParameterTypes.Count; i++)
            {
                if (ParameterTypes[i] != parameters[i])
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"Native Function {_name}";
        }
    }
}