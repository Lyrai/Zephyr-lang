﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Zephyr.Compiling.Contexts;
using Zephyr.Compiling.Reflection;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling
{
    public class ExpressionsCompiler: BaseCompiler
    {
        private readonly Node _tree;
        private MethodInfo _entryPoint = null!;

        public ExpressionsCompiler(Node tree)
        {
            _context = new AssemblyContext("Main", null);
            _tree = tree;
        }

        public MethodInfo Compile()
        {
            _context = _context.DefineModule("Main") ?? throw new InvalidOperationException("Could not define module \"Main\"");
            Visit(_tree);
            //_context.CompleteFunction();
            return _entryPoint;
        }
        
        public override object VisitClassNode(ClassNode n)
        {
            var oldContext = _context;
            _context = _context.DefineType(n.Name, _context.GetTypeByName(n.Parent.Name)) 
                       ?? throw new InvalidOperationException($"Could not define class {n.Name}");
            
            foreach (var node in n.Body)
            {
                Visit(node);
            }

            _context.CreateType();
            _context = oldContext;

            return null!;
        }

        public override object VisitGetNode(GetNode n)
        {
            throw new System.NotImplementedException();
        }

        public override object VisitCompoundNode(CompoundNode n)
        {
            var last = n.GetChildren().Last();
            foreach (var node in n.GetChildren())
            {
                Visit(node);
                if(node is FuncCallNode && node != last)
                    _context.GetILGenerator().Emit(OpCodes.Pop);
            }

            return null!;
        }

        public override object VisitBinOpNode(BinOpNode n)
        {
            var generator = _context.GetILGenerator();
            if (n.Token.Type == TokenType.Assign)
            {
                if (n.Left is VarDeclNode)
                    Visit(n.Left);
                
                Visit(n.Right);
                generator.Emit(OpCodes.Stloc, _context.GetVariable((string) n.Left.Token.Value));
                return null!;
            }
            
            Visit(n.Left);
            Visit(n.Right);

            var op = n.Token.Type switch
            {
                TokenType.Plus => OpCodes.Add,
                TokenType.Minus => OpCodes.Sub,
                TokenType.Multiply => OpCodes.Mul,
                TokenType.Divide => OpCodes.Div,
                TokenType.Equal => OpCodes.Ceq,
                TokenType.NotEqual => OpCodes.Ceq,
                TokenType.Greater => OpCodes.Cgt,
                TokenType.Less => OpCodes.Clt
            };

            generator.Emit(op);
            
            if (n.Token.Type == TokenType.NotEqual)
            {
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ceq);
            }
            
            return null!;
        }

        public override object VisitUnOpNode(UnOpNode n)
        {
            var generator = _context.GetILGenerator()!;
            if (n.Token.Type == TokenType.Print)
            {
                Type[] wlParams = {typeof(object)};
                MethodInfo wrln = typeof(Console).GetMethod("WriteLine", wlParams);
                Visit(n.Operand);
                generator.Emit(OpCodes.Box, MapType(n.Operand.TypeSymbol));
                generator.EmitCall(OpCodes.Call, wrln, null);
            }
            else if (n.Token.Type == TokenType.Minus)
            {
                Visit(n.Operand);
                generator.Emit(OpCodes.Neg);
            }
            else
            {
                throw new InvalidOperationException($"Invalid unary operation: {n.Token.Value}");
            }

            return null!;
        }

        public override object VisitLiteralNode(LiteralNode n)
        {
            var generator = _context.GetILGenerator();
            if (generator is null)
            {
                throw new InvalidOperationException($"Statement in illegal context: {_context}");
            }
            
            switch (n.Token.Type)
            {
                case TokenType.Integer:
                    generator.Emit(OpCodes.Ldc_I4, (int) n.Value);
                    break;
                case TokenType.DoubleLit:
                    generator.Emit(OpCodes.Ldc_R8, (double) n.Value);
                    break;
                case TokenType.True:
                    generator.Emit(OpCodes.Ldc_I4, 1);
                    break;
                case TokenType.False:
                    generator.Emit(OpCodes.Ldc_I4, 0);
                    break;
                case TokenType.StringLit:
                    generator.Emit(OpCodes.Ldstr, (string) n.Value);
                    break;
            }
            
            return null!;
        }

        public override object VisitIfNode(IfNode n)
        {
            Visit(n.Condition);
            var generator = _context.GetILGenerator();
            var elseLabel = generator.DefineLabel();
            var doneLabel = generator.DefineLabel();
            generator.Emit(OpCodes.Brfalse, elseLabel);
            
            Visit(n.ThenBlock);
            
            generator.Emit(OpCodes.Br, doneLabel);
            generator.MarkLabel(elseLabel);
            
            if (n.ElseBlock is not null)
                Visit(n.ElseBlock);

            generator.MarkLabel(doneLabel);
            return null!;
        }

        public override object VisitWhileNode(WhileNode n)
        {
            var generator = _context.GetILGenerator();
            var begin = generator.DefineLabel();
            generator.MarkLabel(begin);
            Visit(n.Condition);
            var done = generator.DefineLabel();
            generator.Emit(OpCodes.Brfalse, done);
            Visit(n.Body);
            generator.Emit(OpCodes.Br, begin);
            generator.MarkLabel(done);

            return null!;
        }

        public override object VisitVarNode(VarNode n)
        {
            _context.GetILGenerator().Emit(OpCodes.Ldloc, _context.GetVariable(n.Name));
            
            return null!;
        }

        public override object VisitVarDeclNode(VarDeclNode n)
        {
            _context.DefineVariable(n.Variable.Name, MapType(n.TypeSymbol));

            return null!;
        }

        public override object VisitPropertyDeclNode(PropertyDeclNode n)
        {
            throw new System.NotImplementedException();
        }

        public override object VisitFuncCallNode(FuncCallNode n)
        {
            var generator = _context.GetILGenerator();
            var args = n.Arguments.Select(node => node.TypeSymbol).Select(MapType).ToList();
            var function = _context.GetFunction(n.Name, args);
            foreach (var node in n.Arguments)
                Visit(node);
            
            generator.EmitCall(OpCodes.Call, function, null);
            return null!;
        }

        public override object VisitFuncDeclNode(FuncDeclNode n)
        {
            var types = new List<Type>();
            foreach (var symbol in n.Symbol.Parameters)
                types.Add(MapType(symbol.Type));
            
            
            var oldContext = _context;
            _context = _context.DefineFunction(n.Name, types, MapType(n.Symbol.ReturnType)) ?? throw new InvalidOperationException($"Cannot define function in {_context}");
            foreach (var node in n.Body)
                Visit(node);

            if (n.ReturnType == "void" && n.Body.Last() is not ReturnNode)
                _context.GetILGenerator().Emit(OpCodes.Ret);

            _context = oldContext;
            if (_context is ModuleContext)
                _context.CompleteFunctions();
            if (_context is ModuleContext && n.Name == "main")
                _entryPoint = _context.LastFunction()!;
            
            return null!;
        }

        public override object VisitReturnNode(ReturnNode n)
        {
            _context.GetILGenerator()?.Emit(OpCodes.Ret);
            return null!;
        }

        public override object VisitNoOpNode(NoOpNode n)
        {
            return null!;
        }

        private object Visit(Node n)
        {
            return n.Accept(this);
        }
    }
}