using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zephyr.Interpreting;

namespace Zephyr.SemanticAnalysis.Symbols
{
    public enum ScopeType
    {
        TopLevel,
        Function,
        Loop,
        Class
    }
    
    public class ScopedSymbolTable : IEnumerable<KeyValuePair<string, object>>, IEnumerator<KeyValuePair<string, object>>
    {
        public ScopedSymbolTable Parent { get; }
        public ScopeType Type { get; }
        private readonly List<KeyValuePair<string, object>> _table;
        private ILookup<string, object> Lookup => _table.ToLookup(k => k.Key, v => v.Value);

        public ScopedSymbolTable(ScopeType type, ScopedSymbolTable parent = null)
        {
            _table = new();
            if (parent == null)
                Init();
            var findFunc = new NativeFunction("findFunc",
                (_, list) => FindFunc(list[0].ToString(), new List<TypeSymbol> {new(list[1].ToString())}))
            {
                ReturnType = new TypeSymbol("function"),
                ParameterTypes = new List<TypeSymbol> {new("string"), new("string")}
            };
            Add("findFunc", findFunc);
            Parent = parent;
            Type = type;
        }

        private void Init()
        {
            Add("int", new BuiltInSymbol("int"));
            Add("double", new BuiltInSymbol("double"));
            Add("bool", new BuiltInSymbol("bool"));
            Add("string", new BuiltInSymbol("string"));
            Add("void", new BuiltInSymbol("void"));
            Add("function", new BuiltInSymbol("function"));
            
            var clockFunction = new NativeFunction("clock", (_, _) => DateTime.Now.ToString(CultureInfo.InvariantCulture))
            {
                ReturnType = new TypeSymbol("string"),
                ParameterTypes = new List<TypeSymbol>()
            };
            Add("clock", clockFunction);

            

            var write = new NativeFunction("write", (_, list) =>
            {
                Console.Write(list[0]);
                return null;
            })
            {
                ReturnType = new TypeSymbol("void"),
                ParameterTypes = new List<TypeSymbol> {new ("string")}
            };
            
            var writeln = new NativeFunction("write", (_, list) =>
            {
                Console.WriteLine(list[0]);
                return null;
            })
            {
                ReturnType = new TypeSymbol("void"),
                ParameterTypes = new List<TypeSymbol> {new ("string")}
            };
            
            Add("write", write);
            Add("writeln", writeln);
        }

        public void Add(string id, object symbol)
        {
            _table.Add(new KeyValuePair<string, object>(id, symbol));
        }

        public ICallable FindFunc(string id, List<TypeSymbol> parameters)
        {
            var symbols = Lookup[id].ToList();
            
            if (symbols.Any())
            {
                foreach (var symbol in symbols)
                {
                    if (symbol is ICallable result && result.TypesEqual(parameters))
                        return result;
                }
            }
            
            return Parent?.FindFunc(id, parameters);
        }

        public T Find<T>(string id) where T : class
        {
            var symbols = Lookup[id].ToList();
            
            if (symbols.Any())
            {
                foreach (var symbol in symbols)
                {
                    if (symbol is T result)
                        return result;
                }
            }

            return Parent?.Find<T>(id);
        }

        public ICallable GetFunc(string id, List<TypeSymbol> parameters)
        {
            var symbols = Lookup[id].ToList();
            
            if (symbols.Any())
            {
                foreach (var symbol in symbols)
                {
                    if (symbol is ICallable result && result.TypesEqual(parameters))
                        return result;
                }
            }

            return null;
        }

        public T Get<T>(string id) where T : class
        {
            var symbols = Lookup[id].ToList();
            
            if (symbols.Any())
            {
                foreach (var symbol in symbols)
                {
                    if (symbol is T result)
                        return result;
                }
            }
            
            return null;
        }

        public void Print()
        {
            Console.WriteLine($"Table has {_table.Count} elements");
            foreach (var (key, value) in _table)
            {
                Console.Write($"{key} : {value}");
                if(value is FuncSymbol symbol)
                {
                    Console.Write(" ");
                    foreach (var symbolParameter in symbol.Parameters)
                    {
                        Console.Write($"{symbolParameter.Type.Name} ");
                    }
                }

                Console.WriteLine();
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _table.Count;
        public KeyValuePair<string, object> Current => _table[_position];
        object IEnumerator.Current => Current;
        private int _position = -1;
            
        public bool MoveNext()
        {
            if (_position + 1 < _table.Count)
            {
                _position++;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _position = -1;
        }
            
        public void Dispose()
        {
            _position = -1;
        }
    }
}