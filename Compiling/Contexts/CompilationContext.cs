using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Zephyr.Compiling.Contexts
{
    public abstract class CompilationContext
    {
        public CompilationContext? Parent { get; }
        private string? _name;

        protected CompilationContext(string? name, CompilationContext? parent)
        {
            _name = name;
            Parent = parent;
        }

        public virtual string? Name()
        {
            return _name;
        }

        public virtual Type? GetTypeByName(string name) => null;

        public override string ToString()
        {
            return "Compilation context";
        }

        public virtual CompilationContext? DefineModule(string name) => null;
        public virtual CompilationContext? DefineType(string name, Type? parent = null) => null;
        public virtual Type? CreateType() => null;
        public virtual CompilationContext? DefineFunction(string name, List<Type> parameters, Type returnType) => null;
        public virtual MethodInfo? LastFunction() => null;
        public virtual MethodInfo? GetFunction(string name, List<Type> parameters) => null;
        public virtual int DefineVariable(string name, Type type) => -1;
        public virtual ILGenerator? GetILGenerator() => null;
        public virtual int GetVariable(string name) => -1;
        public virtual void CompleteFunction() { }
    }
}