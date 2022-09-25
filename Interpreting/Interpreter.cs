using System;
using System.Collections.Generic;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SyntaxAnalysis.ASTNodes;
using static Zephyr.Interpreting.RuntimeValue;

namespace Zephyr.Interpreting
{
    public class Interpreter : INodeVisitor<RuntimeValue>
    {
        private readonly List<Node> _nodes;
        private readonly Scope _globals;
        private Scope _currentScope;

        public Interpreter(List<Node> nodes)
        {
            _nodes = nodes;
            _globals = new Scope();
            _currentScope = _globals;
        }

        public void Interpret()
        {
            foreach (var node in _nodes)
                Evaluate(node);
        }

        public RuntimeValue VisitClassNode(ClassNode n)
        {
            return new(null);
        }

        public RuntimeValue VisitGetNode(GetNode n)
        {
            var obj = Evaluate(n.Obj);
            if (obj.Value is Instance instance)
                return new(instance.Get((string) n.Token.Value));

            throw new ArgumentException("Cannot access member of non-instance");
        }

        public RuntimeValue VisitCompoundNode(CompoundNode n)
        {
            _currentScope = new Scope(_currentScope);
            
            foreach (var child in n.GetChildren())
            {
                try
                {
                    if (child is ReturnNode)
                        return Evaluate(child);

                    Evaluate(child);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message} at line {child.Token.Line}");
                    Console.WriteLine(e.StackTrace);
                }
            }
            
            _currentScope = _currentScope.Parent;
            return None;
        }

        public RuntimeValue VisitBinOpNode(BinOpNode n)
        {
            RuntimeValue right;
            if (n.Token.Type == TokenType.Assign)
            {
                var name = (string) n.Left.Token.Value;
                if (n.Left is GetNode node)
                {
                    var instance = Evaluate(node.Obj).Value as Instance;
                    right = Evaluate(n.Right);
                    return instance.Assign(name, right);
                }

                if (n.Left is VarDeclNode)
                    Evaluate(n.Left);
                
                right = Evaluate(n.Right);
                return _currentScope.Assign(name, right);
                
            }
            
            var left = Evaluate(n.Left);
            right = Evaluate(n.Right);
            return n.Token.Type switch
            {
                TokenType.Plus => left + right,
                TokenType.Minus => left - right,
                TokenType.Multiply => left * right,
                TokenType.Divide => left / right,
                TokenType.Equal => left.CompareTo(right) == 0,
                TokenType.NotEqual => left.CompareTo(right) != 0,
                TokenType.Less => left.CompareTo(right) < 0,
                TokenType.LessEqual => left.CompareTo(right) <= 0,
                TokenType.Greater => left.CompareTo(right) > 0,
                TokenType.GreaterEqual => left.CompareTo(right) >= 0
            };
        }

        public RuntimeValue VisitUnOpNode(UnOpNode n)
        {
            var operand = Evaluate(n.Operand);
            
            if (n.Token.Type == TokenType.Print)
            {
                Console.WriteLine(operand);
                return None;
            }
            
            return n.Token.Type switch
            {
                TokenType.Minus => -operand,
                TokenType.Plus => operand
            };
        }

        public RuntimeValue VisitLiteralNode(LiteralNode n)
        {
            return new(n.Value);
        }

        public RuntimeValue VisitIfNode(IfNode n)
        {
            var condition = Evaluate(n.Condition);
            if ((bool) condition.Value)
                return Evaluate(n.ThenBlock);
            
            if (n.ElseBlock is not null)
                return Evaluate(n.ElseBlock);

            return None;
        }

        public RuntimeValue VisitWhileNode(WhileNode n)
        {
            while ((bool) Evaluate(n.Condition).Value)
                Evaluate(n.Body);

            return None;
        }

        public RuntimeValue VisitVarNode(VarNode n)
        {
            var name = (string) n.Token.Value;
            return _currentScope.Get(name);
        }

        public RuntimeValue VisitVarDeclNode(VarDeclNode n)
        {
            var name = (string) n.Token.Value;
            _currentScope.Define(name);
            return None;
        }

        public RuntimeValue VisitPropertyDeclNode(PropertyDeclNode n)
        {
            Evaluate(n.Variable);
            return None;
        }

        public RuntimeValue VisitFuncCallNode(FuncCallNode n)
        {
            object callee = Evaluate(n.Callee);
            if (((RuntimeValue) callee).IsNone == false)
                callee = ((RuntimeValue) callee).Value;
            
            var callable = n.Callable ?? callee as ICallable;

            if (callable is null)
                throw new ArgumentException("Cannot call non-function");

            var arguments = new List<object>();
            foreach (var argument in n.Arguments)
            {
                arguments.Add(Evaluate(argument).Value);
            }
            

            return new(callable.Call(this, arguments));
        }

        public RuntimeValue VisitFuncDeclNode(FuncDeclNode n)
        {
            n.Symbol.Closure = _currentScope;
            _currentScope.Define(n.Name);
            _currentScope.Assign(n.Name, new RuntimeValue(n.Symbol));
            return None;
        }

        public RuntimeValue VisitReturnNode(ReturnNode n)
        {
            throw new ReturnException(Evaluate(n.Value));
        }

        public RuntimeValue VisitNoOpNode(NoOpNode n)
        {
            return None;
        }

        public void ExecuteBlock(List<Node> body, Scope scope)
        {
            var enclosing = _currentScope;
            _currentScope = scope;

            try
            {
                foreach (var node in body)
                {
                    try
                    {
                        Evaluate(node);
                    }
                    catch (ReturnException e)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e.Message} at line {node.Token.Line}");
                    }
                }
            }
            finally
            {
                _currentScope = enclosing;
            }
        }

        private RuntimeValue Evaluate(Node n)
        {
            return n.Accept(this);
        }
    }
}