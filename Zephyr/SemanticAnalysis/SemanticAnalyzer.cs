using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zephyr.Interpreting;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SemanticAnalysis.Symbols;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.SemanticAnalysis
{
    public class SemanticAnalyzer : INodeVisitor<object>
    {
        private readonly Node _node;
        private readonly TypeSymbol _doubleSymbol;
        private readonly TypeSymbol _intSymbol;
        private readonly TypeSymbol _boolSymbol;
        private readonly TypeSymbol _stringSymbol;
        private readonly TypeSymbol _voidSymbol;
        private ScopedSymbolTable _table;
        private ClassSymbol _currentClassSymbol;

        private static readonly HashSet<TokenType> ComparisonOperators = new()
        {
            TokenType.Equal, 
            TokenType.NotEqual, 
            TokenType.Less, 
            TokenType.LessEqual, 
            TokenType.Greater, 
            TokenType.GreaterEqual
        };

        public SemanticAnalyzer(Node node)
        {
            _node = node;
            _table = new ScopedSymbolTable(ScopeType.TopLevel);
            _intSymbol = _table.Find<TypeSymbol>("int");
            _doubleSymbol = _table.Find<TypeSymbol>("double");
            _boolSymbol = _table.Find<TypeSymbol>("bool");
            _stringSymbol = _table.Find<TypeSymbol>("string");
            _voidSymbol = _table.Find<TypeSymbol>("void");
        }

        public void Analyze()
        {
            Visit(_node);
            var usageAnalyzer = new UsageAnalyzer();
            usageAnalyzer.Analyze(_node);
        }

        public object VisitVarDeclNode(VarDeclNode n)
        {
            var name = (string)n.Variable.Token.Value;
            TypeSymbol typeSymbol = null;
            if(n.Type is not null)
            {
                var type = (string)n.Type.Token.Value;
                typeSymbol = _table.Find<TypeSymbol>(type);

                if (typeSymbol == _voidSymbol)
                    throw new SemanticException(n, "Variable cannot be of type void");
            }

            if (_table.Get<VarSymbol>(name) is not null)
                throw new DuplicateIdentifierException(n.Variable);
            
            var symbol = new VarSymbol(name, typeSymbol);
            n.SetType(typeSymbol);
            _table.Add(name, symbol);
            _currentClassSymbol?.Fields.Add(name, symbol);

            return symbol;
        }

        public object VisitPropertyDeclNode(PropertyDeclNode n)
        {
            if (_table.Type != ScopeType.Class)
                throw new SemanticException(n, "Property cannot be declared outside a class");
            
            var typeSymbol = TypeSymbol.FromObject(Visit(n.Variable));
            if (n.Getter is null && n.Setter is null)
                throw new SemanticException(n, "Property must declare at least one accessor");
            
            if(n.Getter is not null)
                Visit(n.Getter);
            if (n.Setter is not null)
                Visit(n.Setter);
            
            var symbol = new PropertySymbol(n, typeSymbol);
            _table.Add(n.Name, symbol);
            PropertyAccessReplacer replacer = new(_node, symbol);
            replacer.Replace();

            return typeSymbol;
        }

        public object VisitFuncDeclNode(FuncDeclNode n)
        {
            var name = n.Name;

            _table = new ScopedSymbolTable(ScopeType.Function, _table);

            var parameters = n.Parameters.Select(nodeParameter => Visit(nodeParameter) as VarSymbol).ToList();

            var symbol = new FuncSymbol
            {
                Name = name,
                ReturnType = new TypeSymbol(n.ReturnType),
                Parameters = parameters,
                Body = n.Body,
                Type = new TypeSymbol("function")
            };

            var parameterTypes = parameters.Select(x => x.Type).ToList();
            
            if (_table.Parent.GetFunc(name, parameterTypes) is not null)
                throw new DuplicateIdentifierException(n);
            
            _table.Parent.Add(name, symbol);
            _currentClassSymbol?.Methods.Add(name, symbol);
            Visit(n.Body);
            _table = _table.Parent;
            n.Symbol = symbol;
            
            return null;
        }

        public object VisitFuncCallNode(FuncCallNode n)
        {
            var arguments = new List<TypeSymbol>();
            foreach (var t in n.Arguments)
            {
                var type = Visit(t) as TypeSymbol;
                arguments.Add(type);
            }

            ICallable symbol;
            switch (n.Callee)
            {
                case VarNode:
                    var classSymbol = _table.Find<ClassSymbol>((string) n.Callee.Token.Value);
                    if (classSymbol is not null)
                    {
                        EvaluateConstructor(classSymbol, arguments, n);

                        n.Callable = classSymbol;
                        return _table.Find<TypeSymbol>((string) n.Callee.Token.Value);
                    }

                    symbol = _table.FindFunc((string) n.Callee.Token.Value, arguments);
                    if(symbol is null)
                    {
                        Symbol s = _table.Find<VarSymbol>(n.Name);
                        if(s is null)
                            s = _table.Find<FuncSymbol>(n.Name);
                        
                        if (s is not null && s.Type == _table.Find<TypeSymbol>("function"))
                            return _table.Find<TypeSymbol>("function");
                        
                        throw new SemanticException(n, $"Cannot call non-function {n.Name}");
                    }
                    break;
                case FuncCallNode:
                    var type = Visit(n.Callee) as TypeSymbol;
                    if (type != _table.Find<TypeSymbol>("function"))
                        throw new SemanticException(n.Callee, "");

                    return _table.Find<TypeSymbol>("function");
                case GetNode node:
                    if (n.Name == "init")
                        throw new SemanticException(n, "Cannot call constructor directly");
                    
                    type = Visit(node.Obj) as TypeSymbol;
                    var sym = _table.Find<ClassSymbol>(type.Name);
                    if (sym is not null)
                    {                    
                        var methodSymbol = ResolveMethod(n.Name, sym);
                        if(methodSymbol is not null)
                        {
                            n.Callable = methodSymbol;
                            return n.Callable.ReturnType;
                        }
                    }
                    
                    arguments.Insert(0, type);
                    var funcSymbol = _table.FindFunc(n.Name, arguments);
                    if (funcSymbol is not null)
                    {
                        n.Callable = funcSymbol;
                        n.Callee = new VarNode(n.Callee.Token, false);
                        n.Arguments.Insert(0, node.Obj);


                        return n.Callable.ReturnType;
                    }
                    
                    var varSymbol = _table.Find<VarSymbol>(n.Name);
                    if (varSymbol is not null)
                    {
                        if (varSymbol.Type != _table.Find<TypeSymbol>("function"))
                            throw new SemanticException(n, $"Cannot find function or method {n.Name}");
                        
                        n.Callee = new VarNode(n.Callee.Token, false);
                        n.Arguments.Insert(0, node.Obj);
                        return varSymbol.Type;
                    }

                    arguments.RemoveAt(0);
                    var netType = GetNetType((n.Callee as GetNode).Obj.TypeSymbol.Name);
                    var members = netType.GetMember(n.Name).Cast<MethodInfo>();
                    if (!members.Any())
                    {
                        throw new SemanticException(n, "Cannot find method");
                    }

                    var method = members.First(
                        method => method
                            .GetParameters()
                            .Select(param => _table.FindByNetName(param.ParameterType.Name))
                            .SequenceEqual(arguments)
                    );
                    var netMethodType = _table.FindByNetName(method.ReturnParameter.ParameterType.Name);
                    n.SetType(netMethodType);
                    return netMethodType;

                default:
                    throw new SemanticException(n, "Unknown exception");
            }
            
            n.Callable = symbol;
            n.SetType(symbol.ReturnType);

            return symbol.ReturnType;
        }
        
        private void EvaluateConstructor(ClassSymbol classSymbol, List<TypeSymbol> arguments, Node n)
        {
            if(classSymbol.Methods.ContainsKey("init"))
            {
                ICallable symbol = classSymbol.Methods["init"];
                if (symbol.TypesEqual(arguments) == false)
                    throw new SemanticException(n, "Cannot find constructor with given parameters");
                            
                if (symbol.ReturnType != _voidSymbol)
                    throw new SemanticException(n, "Constructor cannot return value");
            }
            else
            {
                if (arguments.Count != 0)
                    throw new SemanticException(n, "Cannot find constructor with given parameters");
            }
        }

        private FuncSymbol ResolveMethod(string name, ClassSymbol symbol)
        {
            if (symbol.Methods.ContainsKey(name))
                return symbol.Methods[name];

            if (symbol.Parent is null)
                return null;

            return ResolveMethod(name, symbol.Parent);
        }

        public object VisitIfNode(IfNode n)
        {
            var type = Visit(n.Condition) as TypeSymbol;
            if (type != _boolSymbol)
                throw new SemanticException(n, $"Cannot cast type {type.Name} to bool");
            
            var thenType = TypeSymbol.FromObject(Visit(n.ThenBlock));
            
            if (n.ElseBlock is not null)
            {
                var elseType = TypeSymbol.FromObject(Visit(n.ElseBlock));
                if (thenType != elseType)
                    throw new SemanticException(n, "Incompatible branches types");
            }
            
            n.SetType(thenType);

            return thenType;
        }

        public object VisitWhileNode(WhileNode n)
        {
            var enclosing = _table;
            _table = new ScopedSymbolTable(ScopeType.Loop, _table);
            var type = Visit(n.Condition) as TypeSymbol;
            if (type != _boolSymbol)
                throw new SemanticException(n, $"Cannot cast type {type.Name} to bool");
            
            Visit(n.Body);
            _table = enclosing;
            
            return null;
        }

        public object VisitVarNode(VarNode n)
        {
            var name = (string)n.Token.Value;
            
            var varSymbol = _table.Find<VarSymbol>(name);

            if (varSymbol is not null)
            {
                n.Symbol = varSymbol;
                n.SetType(varSymbol.Type);

                return varSymbol.Type;
            }

            var funcSymbol = _table.Find<FuncSymbol>(name);
            if (funcSymbol is not null)
            {
                n.Symbol = funcSymbol;

                return funcSymbol.Type;
            }

            if (Type.GetType(n.Name) is not null)
                return new TypeSymbol(n.Name);
                
            throw new UnknownIdentifierException(n);
        }

        public object VisitClassNode(ClassNode n)
        {
            var symbol = new ClassSymbol(n.Name);
            _table.Add(n.Name, symbol);
            _table.Add(n.Name, new TypeSymbol(n.Name));
            _table = new ScopedSymbolTable(ScopeType.Class, _table);
            
            var prev = _currentClassSymbol;
            _currentClassSymbol = symbol;
            n.Symbol = symbol;
            
            var thisSymbol = new VarSymbol("this", _table.Find<TypeSymbol>(n.Name));
            n.Symbol.Fields.Add("this", thisSymbol);
            _table.Add("this", thisSymbol);
            
            if (n.Parent is not null)
            {
                var baseSymbol = new VarSymbol("base", _table.Find<TypeSymbol>(n.Parent.Name));
                n.Symbol.Fields.Add("base", baseSymbol);
                _table.Add("base", baseSymbol);
                
                if (n.Parent.Name == n.Name)
                    throw new SemanticException(n, "Class cannot inherit itself");
                
                var parentSymbol = _table.Find<ClassSymbol>(n.Parent.Name);
                if (parentSymbol is null)
                    throw new UnknownIdentifierException(n.Parent);

                n.Symbol.Parent = parentSymbol;
            }
            
            foreach (var node in n.Body)
                Visit(node);

            _table = _table.Parent;
            _currentClassSymbol = prev;
            return null;
        }

        public object VisitGetNode(GetNode n)
        {
            TypeSymbol type;
            try
            {
                type = TypeSymbol.FromObject(Visit(n.Obj));
            }
            catch (UnknownIdentifierException)
            {
                var typeName = n.Token.Value.ToString();
                var oldNode = n;
                while (n.Obj is GetNode node)
                {
                    typeName = node.Token.Value + "." + typeName;
                    n = node;
                }

                typeName = (n.Obj as VarNode).Name + "." + typeName;
                n = oldNode;
                
                if (GetNetType(typeName) is null)
                {
                    throw;
                }
                
                type = new TypeSymbol(typeName);
                _table.Add(typeName, type);
                n.SetType(type);
                return type;
            }
            
            var symbol = _table.Find<ClassSymbol>(type.Name);
            if (symbol is null)
                throw new UnknownIdentifierException(n);
            
            var name = (string)n.Token.Value;
            var fieldType = ResolveName(name, symbol);
            if (fieldType is null)
                throw new SemanticException(n, $"{symbol.Name} does not contain definition for {name}");
                
            n.SetType(fieldType);
            return fieldType;
        }

        private TypeSymbol ResolveName(string name, ClassSymbol symbol)
        {
            if (symbol.Fields.ContainsKey(name))
                return symbol.Fields[name].Type;

            if (symbol.Parent is null)
                return null;

            return ResolveName(name, symbol.Parent);
        }

        public object VisitCompoundNode(CompoundNode node)
        {
            var enclosing = _table;
            _table = new ScopedSymbolTable(enclosing.Type, _table);
            var children = node.GetChildren();
            for (var i = 0; i < children.Count - 1; i++)
            {
                try
                {
                    Visit(children[i]);
                }
                catch (Exception e)
                {
                    Zephyr.Error(e);
                }
            }

            object type = null!;
            try
            {
                type = Visit(children.Last());
            }
            catch (Exception e)
            {
                Zephyr.Error(e);
            }
            
            _table = enclosing;
            node.SetType(TypeSymbol.FromObject(type));

            return type;
        }

        public object VisitReturnNode(ReturnNode n)
        {
            TypeSymbol type;
            if(n.Value is not null)
            {
                type = (TypeSymbol) Visit(n.Value);
                if (_table.Type != ScopeType.Function)
                    throw new SemanticException(n, "Unexpected return statement");
            }
            else
            {
                type = _voidSymbol;
            }
            
            n.SetType(type);

            return type;
        }

        public object VisitLiteralNode(LiteralNode n)
        {
            var type = n.Value switch
            {
                int => _intSymbol,
                double => _doubleSymbol,
                bool => _boolSymbol,
                string => _stringSymbol
            };

            n.SetType(type);
            return type;
        }

        public object VisitBinOpNode(BinOpNode n)
        {
            var left = TypeSymbol.FromObject(Visit(n.Left));
            var right = TypeSymbol.FromObject(Visit(n.Right));
            if (n.Token.Type == TokenType.Assign)
            {
                if (n.Left is VarDeclNode declNode && left is null)
                {
                    var symbol = _table.Get<VarSymbol>(declNode.Variable.Name);
                    symbol.SetType(right);
                    n.Left.SetType(symbol.Type);
                    n.Right.SetType(right);
                    return right;
                }
                
                if (n.Left is not VarNode && n.Left is not VarDeclNode && n.Left is not GetNode)
                    throw new SemanticException(n.Left, $"Cannot assign to non-variable {n.Left.Token.Value}");

                if (CanCast(right, left) == false)
                    throw new SemanticException(n, $"Cannot assign value of type {right} to variable {n.Left.Token.Value} of type {left}");

                n.SetType(left);
                return left;
            }

            var tokenType = n.Token.Type;
            if (ComparisonOperators.Contains(tokenType))
            {
                if (CanCast(left, right) || CanCast(right, left))
                {
                    n.SetType(_boolSymbol);
                    return _boolSymbol;
                }
                
                throw new SemanticException(n, $"Cannot cast type {left} to target type {right}");
            }
            
            if (left == _doubleSymbol || right == _doubleSymbol)
            {
                n.SetType(_doubleSymbol);
                return _doubleSymbol;
            }
            
            if (left == _stringSymbol && right == _stringSymbol)
            {
                n.SetType(_stringSymbol);
                return _stringSymbol;
            }
            
            n.SetType(_intSymbol);
            return _intSymbol;
        }

        public object VisitUnOpNode(UnOpNode n)
        {
            var op = Visit(n.Operand) as TypeSymbol;
            var type = op == _doubleSymbol ? _doubleSymbol : _intSymbol;
            n.SetType(type);
            return type;
        }

        public object VisitNoOpNode(NoOpNode n)
        {
            return null;
        }

        private object Visit(Node n)
        {
            return n.Accept(this);
        }

        private void Visit(List<Node> n)
        {
            for (var i = 0; i < n.Count; i++)
            {
                Visit(n[i]);
            }
        }

        private bool CanCast(TypeSymbol from, TypeSymbol to)
        {
            if (from == _voidSymbol || to == _voidSymbol)
                return false;
            if (from == _stringSymbol && to == _stringSymbol)
                return true;

            return from == to || from == _intSymbol && to == _doubleSymbol || IsParent(to, from);
        }

        private bool IsParent(TypeSymbol baseClass, TypeSymbol forType)
        {
            var type = _table.Find<ClassSymbol>(forType.Name);
            if (type is null)
                return false;

            while (type is not null)
            {
                if (type.Name == baseClass.Name)
                    return true;
                type = type.Parent;
            }

            return false;
        }

        private Type GetNetType(string name)
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(asm => asm.ExportedTypes)
                .FirstOrDefault(t => t.FullName == name);
        }
    }
}
