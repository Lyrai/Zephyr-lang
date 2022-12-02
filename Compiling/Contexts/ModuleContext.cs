using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Zephyr.Compiling.Contexts
{
    public class ModuleContext : CompilationContext
    {
        private ModuleBuilder _builder;
        private string? _lastFunction;

        public ModuleContext(string? name, CompilationContext? parent, ModuleBuilder builder) : base(name, parent)
        {
            _builder = builder;
            _lastFunction = null;
        }

        public override CompilationContext DefineType(string name, Type? parent = null)
        {
            var builder = _builder.DefineType(name, TypeAttributes.Class, parent);
            return new ClassContext(name, this, builder);
        }

        public override string ToString()
        {
            return "module";
        }

        public override CompilationContext? DefineFunction(string name, List<Type> parameters, Type returnType)
        {
            var builder = _builder.DefineGlobalMethod(name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
                CallingConventions.Standard, returnType, parameters.ToArray());
            _lastFunction = name;

            return new FunctionContext(name, this, builder);
        }

        public override MethodInfo? GetFunction(string name, List<Type> parameters)
        {
            return _builder.GetMethod(name);
        }

        public override Type? GetTypeByName(string name)
        {
            var type = _builder.GetType(name);
            if (type is not null)
                return type;

            type = Parent?.GetTypeByName(name);
            if (type is not null)
                return type;
            
            return _builder.GetType(name) ?? Parent?.GetTypeByName(name);
        }

        public override MethodInfo? LastFunction()
        {
            return _builder.GetMethod(_lastFunction);
        }

        public override void CompleteFunctions()
        {
            _builder.CreateGlobalFunctions();
        }
    }
}