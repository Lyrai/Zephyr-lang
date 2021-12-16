using System.Collections.Generic;
using System.Linq;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.Interpreting
{
    public class Instance
    {
        private ClassSymbol Class { get; }
        private readonly Dictionary<string, RuntimeValue> _fields;
        private readonly Dictionary<string, FuncSymbol> _methods;
        private readonly Instance _parent;

        public Instance(ClassSymbol @class)
        {
            Class = @class;
            if(Class.Parent is not null)
                _parent = new Instance(Class.Parent);
            
            _fields = new Dictionary<string, RuntimeValue>(@class.Fields.Select(x =>
                new KeyValuePair<string, RuntimeValue>(x.Key, RuntimeValue.None))) {["this"] = new(this), ["base"] = new(_parent)};
            
            _methods = new Dictionary<string, FuncSymbol>(@class.Methods.Select(x =>
                new KeyValuePair<string, FuncSymbol>(x.Key, x.Value.Bind(this))));
        }

        public object Get(string name)
        {
            if (_fields.ContainsKey(name))
                return _fields[name];

            if (_methods.ContainsKey(name))
                return _methods[name];

            return _parent.Get(name);
        }

        public RuntimeValue Assign(string name, RuntimeValue value)
        {
            if (_fields.ContainsKey(name))
                return _fields[name] = value;

            return _parent.Assign(name, value);
        }

        public override string ToString()
        {
            return $"Class {Class.Name}";
        }
    }
}