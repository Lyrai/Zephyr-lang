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

                if (IsArrayType(type))
                {
                    typeSymbol = _table.GetArrayType(type);
                }
                else
                {
                    typeSymbol = _table.Find<TypeSymbol>(type) ?? _table.FindByNetName(type);

                    if (typeSymbol == _voidSymbol)
                        throw new SemanticException(n, "Variable cannot be of type void");
                }
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
            var bodyType = TypeSymbol.FromObject(Visit(n.Body));
            var returnType = symbol.ReturnType;
            if(bodyType == returnType)
            {
                _table = _table.Parent;
                n.Symbol = symbol;

                return _voidSymbol;
            }

            if (!CanCast(bodyType, returnType))
            {
                throw new SemanticException(n, $"Function must return {returnType}, but returns {bodyType}");
            }

            n.Replace(n.Body, new ConversionNode(bodyType, returnType, n.Body));
            return _voidSymbol;

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
                case VarNode varNode:
                    var classSymbol = _table.Find<ClassSymbol>((string) varNode.Token.Value);
                    if (classSymbol is not null)
                    {
                        EvaluateConstructor(classSymbol, arguments, n);

                        n.Callable = classSymbol;
                        return _table.FindByNetName(classSymbol.ReturnType.GetNetName());
                    }

                    var netClass = _table.FindByNetName(varNode.Name);
                    if (netClass is not null)
                    {
                        classSymbol = _table.Find<ClassSymbol>(varNode.Name);
                        n.SetType(netClass);
                        n.Callable = classSymbol;
                        return netClass;
                    }

                    symbol = _table.FindFunc((string) n.Callee.Token.Value, arguments);
                    if(symbol is null)
                    {
                        var s = _table.Find<VarSymbol>(n.Name) ?? (Symbol)_table.Find<FuncSymbol>(n.Name);

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

                    try
                    {
                        type = Visit(node.Obj) as TypeSymbol;
                    }
                    catch (UnknownIdentifierException)
                    {
                        var netConstructorType = _table.GetNetType(GetQualifiedName(node));
                        if (netConstructorType is null)
                        {
                            throw;
                        }

                        var typeName = netConstructorType.FullName;
                        type = new TypeSymbol(typeName);
                        _table.Add(typeName, netConstructorType);
                        n.SetType(type);
                        n.Callable = new ClassSymbol(typeName);
                        return type;
                    }
                    
                    var sym = _table.Find<ClassSymbol>(type.Name);
                    if (sym is not null)
                    {                    
                        var methodSymbol = ResolveMethod(n.Name, sym);
                        if(methodSymbol is not null)
                        {
                            n.Callable = methodSymbol;
                            n.SetType(methodSymbol.ReturnType);
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
                        n.SetType(n.Callable.ReturnType);

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
                    Type netType = null;
                    if(node.Obj is VarNode vs && _table.Find<VarSymbol>(vs.Name) is not null)
                    {
                        netType = _table.GetNetType(node.Obj.TypeSymbol.GetNetFullName());
                    }
                    else
                    {
                        netType = _table.GetNetType(GetQualifiedName(node.Obj));
                    }

                    if (netType is null)
                    {
                        throw new SemanticException(n, $"Cannot find method {n.Name}");
                    }
                    
                    var members = netType.GetMember(n.Name).Cast<MethodInfo>();
                    var methodInfos = members as MethodInfo[] ?? members.ToArray();
                    
                    if (!methodInfos.Any())
                    {
                        throw new SemanticException(n, $"Cannot find method {n.Name}");
                    }

                    var method = methodInfos.First(
                        method => method
                            .GetParameters()
                            .Select(param => _table.FindByNetName(param.ParameterType.Name))
                            .Zip(arguments, (param, arg) => (param, arg))
                            .All(pair => CanCast(pair.arg, pair.param))
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
            if(classSymbol.Methods.TryGetValue("init", out var symbol))
            {
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
            if (symbol.Methods.TryGetValue(name, out var method))
                return method;

            if (symbol.Parent is null)
                return null;

            return ResolveMethod(name, symbol.Parent);
        }

        public object VisitIfNode(IfNode n)
        {
            var type = Visit(n.Condition) as TypeSymbol;
            if (type != _boolSymbol)
            {
                throw new SemanticException(n, $"Cannot cast type {type.Name} to bool");
            }
            
            var thenType = TypeSymbol.FromObject(Visit(n.ThenBlock));
            n.SetType(thenType);
            
            if (n.ElseBlock is null)
            {
                return thenType;
            }
            
            var elseType = TypeSymbol.FromObject(Visit(n.ElseBlock));
            if (thenType == elseType)
            {
                return thenType;
            }

            if (CanCast(thenType, elseType))
            {
                n.SetType(elseType);
                n.Replace(n.ThenBlock, new ConversionNode(thenType, elseType, n.ThenBlock));
            }
            
            if (CanCast(elseType, thenType))
            {
                n.Replace(n.ElseBlock, new ConversionNode(elseType, thenType, n.ElseBlock));
            }
            
            throw new SemanticException(n, "If expression branches must agree on return type");
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
            
            return _voidSymbol;
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

            var netType = _table.FindByNetName(name);
            if (netType is not null)
            {
                n.SetType(netType);
                return netType;
            }
                
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
            return _voidSymbol;
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
                var typeName = GetQualifiedName(n);
                
                if (_table.GetNetType(typeName) is null)
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
                throw new SemanticException(n, $"`{n}` of type `{type}` does not contain definition for `{n.Token.Value}`");
            
            var name = (string)n.Token.Value;
            var fieldType = ResolveName(name, symbol);
            if (fieldType is null)
                throw new SemanticException(n, $"{symbol.Name} does not contain definition for {name}");
                
            n.SetType(fieldType);
            return fieldType;
        }

        private TypeSymbol ResolveName(string name, ClassSymbol symbol)
        {
            if (symbol.Fields.TryGetValue(name, out var field))
                return field.Type;

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
                    
                    if (_currentClassSymbol is null && children[i] is FuncDeclNode decl)
                    {
                        decl.SetStatic(true);
                    }
                }
                catch (Exception e)
                {
                    Program.Error(e);
                }
            }

            object type = null;
            try
            {
                type = Visit(children.Last());
            }
            catch (Exception e)
            {
                Program.Error(e);
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
                
                if (n.Left is not VarNode && n.Left is not VarDeclNode && n.Left is not GetNode && n.Left is not IndexNode)
                    throw new SemanticException(n.Left, $"Cannot assign to non-variable {n.Left.Token.Value}");

                if (CanCast(right, left) == false)
                    throw new SemanticException(n, $"Cannot assign value of type {right} to variable {n.Left.Token.Value} of type {left}");
                
                if(left != right)
                {
                    n.Replace(n.Right, new ConversionNode(right, left, n.Right));
                }
                

                n.SetType(left);
                return left;
            }

            var tokenType = n.Token.Type;
            if (ComparisonOperators.Contains(tokenType))
            {
                if (!left.IsNumericType() || !right.IsNumericType())
                {
                    throw new SemanticException(n, "Comaprison operands must be numeric");
                }
                
                n.SetType(_boolSymbol);
                return _boolSymbol;
                
            }

            if (left == right)
            {
                n.SetType(left);
                return left;
            }

            if (left == _stringSymbol && n.Token.Type == TokenType.Plus)
            {
                n.SetType(_stringSymbol);
                return _stringSymbol;
            }

            if (CanCast(left, right))
            {
                n.Replace(n.Left, new ConversionNode(left, right, n.Left));
                n.SetType(right);
                return n.TypeSymbol;
            }

            if (CanCast(right, left))
            {
                n.Replace(n.Right, new ConversionNode(right, left, n.Right));
                n.SetType(left);
                return n.TypeSymbol;
            }

            throw new SemanticException(n, $"Cannot cast type {left} to target type {right}");
        }

        public object VisitUnOpNode(UnOpNode n)
        {
            Visit(n.Operand);
            n.SetType(_voidSymbol);
            return _voidSymbol;
        }

        public object VisitNoOpNode(NoOpNode n)
        {
            return null;
        }

        public object VisitArrayInitializerNode(ArrayInitializerNode n)
        {
            TypeSymbol arrType = null;
            var itemsToConvert = new List<Node>();
            foreach (var node in n.GetChildren())
            {
                Visit(node);
                if (arrType is null)
                {
                    arrType = node.TypeSymbol;
                    continue;
                }

                TypeSymbol conversionTo = null;
                if (node.TypeSymbol == arrType)
                {
                    continue;
                }
                
                if (node.TypeSymbol != arrType && !CanCast(node.TypeSymbol, arrType))
                {
                    throw new SemanticException(n, $"Cannot put {node.TypeSymbol.Name} into array of {arrType.Name}");
                }

                itemsToConvert.Add(node);
            }

            foreach (var item in itemsToConvert)
            {
                n.Replace(item, new ConversionNode(item.TypeSymbol, arrType, item));
            }

            var type = _table.GetArrayType(arrType);
            n.SetType(type);

            return type;
        }

        public object VisitIndexNode(IndexNode n)
        {
            var arrayType = Visit(n.Expression) as TypeSymbol;
            if (arrayType is not ArrayTypeSymbol arrayTypeSymbol)
            {
                if(_table.GetNetType(arrayType.GetNetFullName()).GetMember("Item")[0] is not PropertyInfo info)
                {
                    throw new SemanticException(n.Expression, $"Cannot index {arrayType.Name}");
                }

                var elemType = _table.FindByNetName(info.GetMethod.ReturnType.Name);
                n.Expression.SetType(elemType);
                arrayTypeSymbol = _table.GetArrayType(elemType);
            }

            var indexType = Visit(n.Index) as TypeSymbol;
            if (indexType != _intSymbol)
            {
                throw new SemanticException(n.Index, $"Cannot index with {indexType.Name}");
            }

            var elementType = arrayTypeSymbol.ElementType;
            n.SetType(elementType);

            return elementType;
        }

        public object VisitUseNode(UseNode n)
        {
            _table.AddUse(n.Namespace);
            return null;
        }

        public object VisitConversionNode(ConversionNode n)
        {
            if (CanCast(n.From, n.To))
            {
                return n.To;
            }
            
            throw new SemanticException(n, $"Cannot cast type {n.From} to target type {n.To}");
        }

        private object Visit(Node n)
        {
            return n.Accept(this);
        }

        private void Visit(List<Node> n)
        {
            foreach (var node in n)
            {
                Visit(node);
            }
        }

        private bool CanCast(TypeSymbol from, TypeSymbol to)
        {
            if (from == to)
            {
                return true;
            }
            
            if (from == _voidSymbol || to == _voidSymbol)
            {
                return false;
            }
            
            if (from == _stringSymbol && to == _stringSymbol)
            {
                return true;
            }

            var objectSymbol = _table.Find<TypeSymbol>("object");
            if (to == objectSymbol)
            {
                return true;
            }

            if (!from.IsNumericType() || !to.IsNumericType())
            {
                return IsParent(to, from);
            }

            var wider = from.GetWider(to);
            return wider != from;

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

        private string GetQualifiedName(Node n)
        {
            if(n is GetNode getNode)
            {
                var typeName = n.Token.Value.ToString();
                while (getNode.Obj is GetNode node)
                {
                    typeName = node.Token.Value + "." + typeName;
                    getNode = node;
                }

                return (getNode.Obj as VarNode).Name + "." + typeName;
            }
            
            if (n is VarNode varNode)
            {
                return _table.FindByNetName(varNode.Name).GetNetFullName();
            }

            return null;
        }

        private bool IsArrayType(string type)
        {
            return type.StartsWith("[");
        }
    }
}
