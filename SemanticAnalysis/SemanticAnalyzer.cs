using System;
using System.Collections.Generic;
using System.Linq;
using Zephyr.Interpreting;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SemanticAnalysis.Symbols;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.SemanticAnalysis
{
    public class SemanticAnalyzer : INodeVisitor<object>
    {
        private readonly List<Node> _nodes;
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

        public SemanticAnalyzer(List<Node> nodes)
        {
            _nodes = nodes;
            _table = new ScopedSymbolTable(ScopeType.TopLevel);
            _intSymbol = _table.Find<TypeSymbol>("int");
            _doubleSymbol = _table.Find<TypeSymbol>("double");
            _boolSymbol = _table.Find<TypeSymbol>("bool");
            _stringSymbol = _table.Find<TypeSymbol>("string");
            _voidSymbol = _table.Find<TypeSymbol>("void");
        }

        public void Analyze()
        {
            Visit(_nodes);
        }

        public object VisitVarDeclNode(VarDeclNode n)
        {
            var name = (string)n.Variable.Token.Value;
            var type = (string)n.Type.Token.Value;
            var typeSymbol = _table.Find<TypeSymbol>(type);
            
            if (typeSymbol == _voidSymbol)
                throw new SemanticException(n, "Variable cannot be of type void");

            if (_table.Get<VarSymbol>(name) is not null)
                throw new DuplicateIdentifierException(n.Variable);
            
            var symbol = new VarSymbol(name, typeSymbol);
            _table.Add(name, symbol);
            if(_currentClassSymbol is not null)
                _currentClassSymbol.Fields.Add(name, symbol);
            
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
            PropertyAccessReplacer replacer = new(_nodes, symbol);
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
                Body = n.Body
            };

            var parameterTypes = parameters.Select(x => x.Type).ToList();
            
            if (_table.Parent.GetFunc(name, parameterTypes) is not null)
                throw new DuplicateIdentifierException(n);
            
            _table.Parent.Add(name, symbol);
            if(_currentClassSymbol is not null)
                _currentClassSymbol.Methods.Add(name, symbol);
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
                        var s = _table.Find<VarSymbol>(n.Name);
                        
                        if (s is not null && s.Type == _table.Find<TypeSymbol>("function"))
                            return _table.Find<TypeSymbol>("function");
                        
                        throw new SemanticException(n, "Cannot call non-function");
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
                    var methodSymbol = ResolveMethod(n.Name, sym);
                    if (methodSymbol is null)
                        throw new SemanticException(n, $"Class {sym.Name} doesn't contain definition for {n.Name}");

                    n.Callable = methodSymbol;
                    return n.Callable.ReturnType;
                default:
                throw new SemanticException(n, "Unknown exception");
            }
            
            n.Callable = symbol;

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
            
            Visit(n.ThenBlock);
            
            if (n.ElseBlock is not null)
                Visit(n.ElseBlock);

            return null;
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

            if (varSymbol is null)
                throw new UnknownIdentifierException(n);

            n.Symbol = varSymbol;

            return varSymbol.Type;
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
            var type = TypeSymbol.FromObject(Visit(n.Obj));
            var symbol = _table.Find<ClassSymbol>(type.Name);
            if (symbol is not null)
            {
                var name = (string)n.Token.Value;
                var fieldType = ResolveName(name, symbol);
                if (fieldType is not null)
                    return fieldType;

                throw new SemanticException(n, $"{symbol.Name} does not contain definition for {name}");
            }

            throw new UnknownIdentifierException(n);
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
            for (var i = 0; i < children.Count; i++)
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

            _table = enclosing;

            return null;
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
            /*var returnType = _currentScopeSymbols.Peek().Type;
            if (CanCast(type, returnType) == false)
                throw new SemanticException(n, $"Cannot cast type {type} to target type {returnType}");*/

            return type;
        }

        public object VisitLiteralNode(LiteralNode n)
        {
            return n.Value switch
            {
                int => _intSymbol,
                double => _doubleSymbol,
                bool => _boolSymbol,
                string => _stringSymbol
            };
        }

        public object VisitBinOpNode(BinOpNode n)
        {
            var left = TypeSymbol.FromObject(Visit(n.Left));
            var right = Visit(n.Right) as TypeSymbol;
            if (n.Token.Type == TokenType.Assign)
            {
                if (n.Left is not VarNode && n.Left is not VarDeclNode && n.Left is not GetNode)
                    throw new SemanticException(n.Left, $"Cannot assign to non-variable {n.Left.Token.Value}");

                if (CanCast(right, left) == false)
                    throw new SemanticException(n, $"Cannot assign value of type {right} to variable {n.Left.Token.Value} of type {left}");

                return left;
            }

            var tokenType = n.Token.Type;
            if (ComparisonOperators.Contains(tokenType))
            {
                if (CanCast(left, right) || CanCast(right, left))
                    return _boolSymbol;
                
                throw new SemanticException(n, $"Cannot cast type {left} to target type {right}");
            }
            
            if (left == _doubleSymbol || right == _doubleSymbol)
                return _doubleSymbol;
            if (left == _stringSymbol && right == _stringSymbol)
                return _stringSymbol;
            
            return _intSymbol;
        }

        public object VisitUnOpNode(UnOpNode n)
        {
            var op = Visit(n.Operand) as TypeSymbol;
            return op == _doubleSymbol ? _doubleSymbol : _intSymbol;
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

            return from == to || from == _intSymbol && to == _doubleSymbol;
        }
    }
}