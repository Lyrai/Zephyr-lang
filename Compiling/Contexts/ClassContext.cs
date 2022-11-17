using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Zephyr.Compiling.Contexts
{
    public class ClassContext : CompilationContext
    {
        private TypeBuilder _builder;
        private MethodInfo? _currentMethod;
        private Dictionary<string, MethodInfo> _methods;
        private Dictionary<string, FieldInfo> _fields;
        
        public ClassContext(string name, CompilationContext parent, TypeBuilder builder): base(name, parent)
        {
            _builder = builder;
            _currentMethod = null;
            _methods = new Dictionary<string, MethodInfo>();
            _fields = new Dictionary<string, FieldInfo>();
        }

        public override Type? CreateType()
        {
            return _builder.CreateType();
        }

        public override CompilationContext? DefineFunction(string name, List<Type> parameters, Type returnType)
        {
            return base.DefineFunction(name, parameters, returnType);
        }

        public override string ToString()
        {
            return "class";
        }
    }
}