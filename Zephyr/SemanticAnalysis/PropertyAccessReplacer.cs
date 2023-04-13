using System;
using System.Collections.Generic;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SemanticAnalysis.Symbols;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.SemanticAnalysis
{
    public class PropertyAccessReplacer : INodeVisitor<object>
    {
        private readonly Node _node;
        private readonly string _name;
        private readonly PropertySymbol _symbol;

        public PropertyAccessReplacer(Node node, PropertySymbol symbol)
        {
            _node = node;
            _name = symbol.Name;
            _symbol = symbol;
        }

        public void Replace()
        {
            Visit(_node);
        }

        public object VisitClassNode(ClassNode n)
        {
            foreach (var node in n.Body)
            {
                Visit(node);
            }

            return null;
        }

        public object VisitGetNode(GetNode n)
        {
            Visit(n.Obj);
            return null;
        }

        public object VisitCompoundNode(CompoundNode n)
        {
            var children = n.GetChildren();
            VisitNodesList(children);

            return null;
        }

        public object VisitBinOpNode(BinOpNode n)
        {
            if (n.Left is VarNode or GetNode)
            {
                if(TryGetCall(n.Left, out var callNode))
                        n.Replace(n.Left, callNode);
            }
            else
            {
                Visit(n.Left);
            }
            
            if (n.Right is VarNode or GetNode)
            {
                if(TryGetCall(n.Right, out var callNode))
                    n.Replace(n.Right, callNode);
            }
            else
            {
                Visit(n.Right);
            }
            
            return null;
        }

        public object VisitUnOpNode(UnOpNode n)
        {
            if (n.Operand is VarNode or GetNode)
            {
                if (TryGetCall(n.Operand, out var callNode))
                    n.Replace(n.Operand, callNode);
            }
            else
            {
                Visit(n.Operand);
            }

            return null;
        }

        public object VisitLiteralNode(LiteralNode n)
        {
            return null;
        }

        public object VisitIfNode(IfNode n)
        {
            Visit(n.Condition);
            Visit(n.ThenBlock);
            if(n.ElseBlock is not null)
                Visit(n.ElseBlock);

            return null;
        }

        public object VisitWhileNode(WhileNode n)
        {
            Visit(n.Condition);
            Visit(n.Body);
            
            return null;
        }

        public object VisitVarNode(VarNode n)
        {
            return null;
        }

        public object VisitVarDeclNode(VarDeclNode n)
        {
            return null;
        }

        public object VisitPropertyDeclNode(PropertyDeclNode n)
        {
            if(n.Getter is not null)
                Visit(n.Getter);
            if(n.Setter is not null)
                Visit(n.Setter);

            return null;
        }

        public object VisitFuncCallNode(FuncCallNode n)
        {
            var arguments = n.Arguments;
            for (var i = 0; i < arguments.Count; i++)
            {
                if (arguments[i] is VarNode node)
                {
                    if (TryGetCall(node, out var callNode))
                        arguments[i] = callNode;
                }
                else
                {
                    Visit(arguments[i]);
                }
            }

            return null;
        }

        public object VisitFuncDeclNode(FuncDeclNode n)
        {
            VisitNodesList(n.Body);

            return null;
        }

        public object VisitReturnNode(ReturnNode n)
        {
            Visit(n.Value);

            return null;
        }

        public object VisitNoOpNode(NoOpNode n)
        {
            return null;
        }

        public object VisitArrayInitializerNode(ArrayInitializerNode n)
        {
            return null;
        }

        public object VisitIndexNode(IndexNode n)
        {
            return null;
        }

        public object VisitUseNode(UseNode n)
        {
            return null!;
        }

        private void Visit(Node n)
        {
            n.Accept(this);
        }

        private bool TryGetCall(Node node, out FuncCallNode callNode)
        {
            if((string) node.Token.Value == _name)
            {
                if (_symbol.HasGetter == false)
                    throw new SemanticException(node, $"Property {_name} has no getter");
                
                var name = node.Token.Value;
                var callToken = node.Token with {Value = $"get_{name}"};
                callNode = new FuncCallNode(node, callToken, new List<Node>());

                Visit(callNode);

                return true;
            }

            callNode = null;
            return false;
        }

        private bool TrySetCall(BinOpNode node, out FuncCallNode callNode)
        {
            if ((string) node.Left.Token.Value == _name)
            {
                if (_symbol.HasSetter == false)
                    throw new SemanticException(node.Left, $"Property {_name} has no setter");

                var callToken = node.Left.Token with {Value = $"set_{_name}"};
                callNode = new FuncCallNode(node.Left, callToken, new List<Node> {node.Right});
                Visit(callNode);

                return true;
            }

            callNode = null;
            return false;
        }

        private void VisitNodesList(List<Node> nodes)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                try
                {
                    if (nodes[i] is not BinOpNode node || node.Token.Type != TokenType.Assign)
                    {
                        Visit(nodes[i]);
                        continue;
                    }

                    if (node.Right is VarNode or GetNode)
                    {
                        if (TryGetCall(node.Right, out var callNode))
                            node.Replace(node.Right, callNode);
                    }
                    else
                    {
                        Visit(node.Right);
                    }

                    if (TrySetCall(node, out var leftCallNode))
                        nodes[i] = leftCallNode;
                }
                catch (SemanticException e)
                {
                    Program.Error(e);
                }
                catch (Exception e)
                {
                    #if DEBUG
                    Program.Error(e);
                    #endif
                }
            }
        }
    }
}
