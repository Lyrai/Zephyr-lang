using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Roslyn.Utilities;
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
        
        private static Dictionary<string, TypeSymbol> _predefinedTypes = new()
        {
            ["System.Object"] = new TypeSymbol("System.Object"),
            ["System.Console"] = new TypeSymbol("System.Console"),
            ["<global class>"] = new TypeSymbol("<global class>"),
        };

        private List<string> _uses = new();

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
        
        public static Dictionary<string, TypeSymbol> GetPredefinedTypes()
        {
            return _predefinedTypes;
        }

        private void Init()
        {
            Add("int", new BuiltInSymbol("int"));
            Add("double", new BuiltInSymbol("double"));
            Add("bool", new BuiltInSymbol("bool"));
            Add("string", new BuiltInSymbol("string"));
            Add("void", new BuiltInSymbol("void"));
            Add("function", new BuiltInSymbol("function"));
            Add("long", new BuiltInSymbol("long"));
            Add("object", new BuiltInSymbol("object"));
            
            var clockFunction = new NativeFunction("clock", (_, _) => DateTime.Now.ToString(CultureInfo.InvariantCulture))
            {
                ReturnType = Find<TypeSymbol>("string"),
                ParameterTypes = new List<TypeSymbol>()
            };
            Add("clock", clockFunction);

            

            var write = new NativeFunction("write", (_, list) =>
            {
                Console.Write(list[0]);
                return null;
            })
            {
                ReturnType = Find<TypeSymbol>("void"),
                ParameterTypes = new List<TypeSymbol> {Find<TypeSymbol>("string")}
            };
            
            var writeln = new NativeFunction("write", (_, list) =>
            {
                Console.WriteLine(list[0]);
                return null;
            })
            {
                ReturnType = Find<TypeSymbol>("void"),
                ParameterTypes = new List<TypeSymbol> {Find<TypeSymbol>("string")}
            };
            
            Add("write", write);
            Add("writeln", writeln);
        }

        public TypeSymbol FindByNetName(string name)
        {
            var nativeName = name switch
            {
                "Void" or "System.Void" => "void",
                "Boolean" or "System.Boolean" => "bool",
                "Double" or "System.Double" => "double",
                "Int32" or "System.Int32" => "int",
                "Int64" or "System.Int64" => "long",
                "String" or "System.String" => "string",
                "Object" or "System.Object" => "object",
                _ => null
            };

            if (nativeName is null)
            {
                var netType = GetNetType(name);
                if (netType is not null)
                {
                    return AddNetType(netType);
                }
                foreach (var use in _uses)
                {
                    netType = GetNetType(use + "." + name);
                    if (netType is not null)
                    {
                        return AddNetType(netType);
                    }
                }
            }

            return Find<TypeSymbol>(nativeName) ?? Find<TypeSymbol>(name) ?? Parent?.FindByNetName(name);
        }

        public ArrayTypeSymbol GetArrayType(TypeSymbol elemType)
        {
            var name = "[" + elemType.Name + "]";
            return GetArrayType(name, elemType.Name);
        }

        public ArrayTypeSymbol GetArrayType(string arrayType, string? elementType = null)
        {
            var symbol = Find<ArrayTypeSymbol>(arrayType);
            if (symbol is not null)
            {
                return symbol;
            }

            elementType ??= arrayType.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[0];
            symbol = new ArrayTypeSymbol(arrayType, Find<TypeSymbol>(elementType));
            Add(arrayType, symbol);
            return symbol;
        }
        
        public Type GetNetType(string name)
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(asm => asm.ExportedTypes)
                .FirstOrDefault(t => t.FullName == name);
        }

        public void Add(string id, object symbol)
        {
            _table.Add(new KeyValuePair<string, object>(id, symbol));
        }

        public void AddUse(string @namespace)
        {
            _uses.Add(@namespace);
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

        private TypeSymbol AddNetType(Type netType)
        {
            var symbol = new TypeSymbol(netType.FullName);
            Add(netType.FullName, symbol);
            Add(netType.Name, new ClassSymbol(netType.Name));
            return symbol;
        }
    }
}
