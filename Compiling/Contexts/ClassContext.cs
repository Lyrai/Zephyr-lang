using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Zephyr.Compiling.Contexts
{
    public class ClassContext : CompilationContext
    {
        public TypeBuilder Builder { get; }
        private MethodInfo? _currentMethod;
        private Dictionary<string, MethodInfo> _methods;
        private Dictionary<string, FieldInfo> _fields;
        
        public ClassContext(string name, CompilationContext parent, TypeBuilder builder): base(name, parent)
        {
            Builder = builder;
            _currentMethod = null;
            _methods = new Dictionary<string, MethodInfo>();
            _fields = new Dictionary<string, FieldInfo>();
        }
        
        public MethodInfo? CurrentMethod()
        {
            return _currentMethod;
        }

        public MethodInfo? GetMethod(string name)
        {
            var hasMethod = _methods.TryGetValue(name, out var method);
            return hasMethod ? method : null;
        }

        public FieldInfo? GetField(string name)
        {
            var hasField = _fields.TryGetValue(name, out var field);
            return hasField ? field : null;
        }

        public void AddMethod(string name, MethodInfo info)
        {
            _methods[name] = info;
        }

        public void AddField(string name, FieldInfo info)
        {
            _fields[name] = info;
        }
    }
}