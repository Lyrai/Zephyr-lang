using System;
using System.Collections.Generic;

namespace Zephyr.Interpreting
{
    public class Scope
    {
        public Scope Parent { get; }
        private readonly Dictionary<string, RuntimeValue> _table;

        public Scope(Scope parent = null)
        {
            Parent = parent;
            _table = new Dictionary<string, RuntimeValue>();
        }

        public void Define(string name)
        {
            _table.Add(name, RuntimeValue.None);
        }

        public RuntimeValue Assign(string name, RuntimeValue value)
        {
            if(_table.ContainsKey(name))
                return _table[name] = value;
            
            if (Parent is null)
                throw new ArgumentException($"Parent is null, name {name}");

            return Parent.Assign(name, value);
        }

        public RuntimeValue Get(string name)
        {
            if (_table.ContainsKey(name))
                return _table[name];

            if(Parent is null)
                return RuntimeValue.None;

            return Parent.Get(name);
        }

        public bool Contains(string name)
        {
            return Get(name).IsNone;
        }

        public void Print()
        {
            foreach (var (key, val) in _table)
            {
                Console.WriteLine($"{key} : {val}");
            }
            
            Parent?.Print();
        }
    }
}