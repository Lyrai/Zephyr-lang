using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Zephyr.Compiling.Contexts
{
    public class AssemblyContext: CompilationContext
    {
        private AssemblyBuilder _builder;
        
        public AssemblyContext(string name, CompilationContext? context) : base(name, context)
        {
            _builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);
        }

        public override CompilationContext DefineModule(string name)
        {
            var builder = _builder.DefineDynamicModule(name);
            return new ModuleContext(name, this, builder);
        }

        public override string ToString()
        {
            return "assembly";
        }

        public override Type? GetTypeByName(string name)
        {
            return Type.GetType($"System.{name}") ?? Type.GetType($"System.Collections.Generic.{name}") ?? throw new ArgumentException($"Unknown type {name}");
        }
    }
}