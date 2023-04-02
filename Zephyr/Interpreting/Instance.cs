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

            _fields = @class.Fields.ToDictionary(pair => pair.Key, _ => RuntimeValue.None);
            _fields.Add("this", new(this));
            _fields.Add("base", new(_parent));

            _methods = @class.Methods.ToDictionary(pair => pair.Key, pair => pair.Value.Bind(this));
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
