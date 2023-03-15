using System.Collections.Generic;
using Zephyr.Interpreting;

namespace Zephyr.SemanticAnalysis.Symbols
{
    public class ClassSymbol : Symbol, ICallable
    {
        public TypeSymbol ReturnType { get; }
        public Dictionary<string, VarSymbol> Fields { get; }
        public Dictionary<string, FuncSymbol> Methods { get; }
        public ClassSymbol Parent { get; set; }
        
        public ClassSymbol(string name) : base(name)
        {
            ReturnType = new TypeSymbol(name);
            Fields = new Dictionary<string, VarSymbol>();
            Methods = new Dictionary<string, FuncSymbol>();
        }

        public int Arity()
        {
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new Instance(this);

            if (Parent is not null && Parent.Methods.ContainsKey("init"))
                Parent.Methods["init"].Call(interpreter, arguments);
            
            if (Methods.ContainsKey("init"))
                Methods["init"].Call(interpreter, arguments);

            return instance;
        }

        public bool TypesEqual(List<TypeSymbol> parameters)
        {
            return true;
        }
    }
}