using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Zephyr.Compiling.Contexts
{
    public class FunctionContext: CompilationContext
    {
        private MethodBuilder _builder;
        private ILGenerator _generator;
        private Dictionary<string, int> _locals;

        public FunctionContext(string? name, CompilationContext? parent, MethodBuilder builder) : base(name, parent)
        {
            _builder = builder;
            _generator = _builder.GetILGenerator();
            _locals = new Dictionary<string, int>();
        }

        public override int DefineVariable(string name, Type type)
        {
            var idx = _builder.GetILGenerator().DeclareLocal(type).LocalIndex;
            _locals[name] = idx;
            return idx;
        }

        public override ILGenerator? GetILGenerator()
        {
            return _generator;
        }

        public override int GetVariable(string name)
        {
            if(_locals.ContainsKey(name))
                return _locals[name];

            throw new ArgumentException($"Cannot find local variable {name}");
        }

        public override MethodInfo? GetFunction(string name, List<Type> parameters)
        {
            return Parent.GetFunction(name, parameters);
        }
    }
}