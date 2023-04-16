using System;
using System.Collections.Generic;
using System.Linq;
using Zephyr.Interpreting;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.SemanticAnalysis.Symbols
{
    public class FuncSymbol : Symbol, ICallable
    {
        public List<VarSymbol> Parameters { get; init; }
        public Node Body { get; init; }
        public TypeSymbol ReturnType { get; init; }
        public Scope Closure { get; set; }

        public bool TypesEqual(List<TypeSymbol> types)
        {
            if (Parameters.Count != types.Count)
                return false;

            return !Parameters.Where((t, i) => t.Type != types[i]).Any();
        }

        public int Arity()
        {
            return Parameters.Count;
        }

        public FuncSymbol Bind(Instance instance)
        {
            var closure = new Scope(Closure);
            closure.Define("this");
            closure.Assign("this", new(instance));
            Closure = closure;
            return this;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments.Count != Arity())
                throw new ArgumentException($"Wrong number of arguments to call: expected {Arity()}, got {arguments.Count}");
            
            var scope = new Scope(Closure);
            for (var i = 0; i < arguments.Count; i++)
            {
                scope.Define(Parameters[i].Name);
                scope.Assign(Parameters[i].Name, new(arguments[i]));
            }

            try
            {
                interpreter.ExecuteBlock(new List<Node> {Body}, scope);
            }
            catch (ReturnException e)
            {
                return e.Value;
            }
            
            return RuntimeValue.None;
        }

        public override string ToString()
        {
            return $"Function {Name}";
        }
    }
}
