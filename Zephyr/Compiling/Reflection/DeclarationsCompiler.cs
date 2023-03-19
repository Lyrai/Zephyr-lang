using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Zephyr.Compiling;
using Zephyr.Compiling.Contexts;
using Zephyr.Compiling.Reflection;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling
{

    public class DeclarationsCompiler : BaseCompiler
    {
        private Node _tree;

        public DeclarationsCompiler(Node tree)
        {
            _tree = tree;
            _context = _context = new AssemblyContext("Main", null);
        }

        public CompilationContext Compile()
        {
            _context = _context.DefineModule("Main") ??
                       throw new InvalidOperationException("Could not define module \"Main\"");
            Visit(_tree);
            _context.CompleteFunctions();
            return _context;
        }

        public override object VisitClassNode(ClassNode n)
        {
            _context = _context.DefineType(n.Name, _context.GetTypeByName(n.Parent.Name))
                       ?? throw new InvalidOperationException($"Could not define class {n.Name}");

            foreach (var node in n.Body)
            {
                Visit(node);
            }

            _context.CreateType();
            _context = _context.Parent;

            return null!;
        }

        public override object VisitGetNode(GetNode n)
        {
            return null!;
        }

        public override object VisitCompoundNode(CompoundNode n)
        {
            throw new System.NotImplementedException();
        }

        public override object VisitBinOpNode(BinOpNode n)
        {
            return null!;
        }

        public override object VisitUnOpNode(UnOpNode n)
        {
            return null!;
        }

        public override object VisitLiteralNode(LiteralNode n)
        {
            return null!;
        }

        public override object VisitIfNode(IfNode n)
        {
            return null!;
        }

        public override object VisitWhileNode(WhileNode n)
        {
            return null!;
        }

        public override object VisitVarNode(VarNode n)
        {
            return null!;
        }

        public override object VisitVarDeclNode(VarDeclNode n)
        {
            return null!;
        }

        public override object VisitPropertyDeclNode(PropertyDeclNode n)
        {
            return null!;
        }

        public override object VisitFuncCallNode(FuncCallNode n)
        {
            return null!;
        }

        public override object VisitFuncDeclNode(FuncDeclNode n)
        {
            var types = new List<Type>();
            foreach (var symbol in n.Symbol.Parameters)
                types.Add(MapType(symbol.Type));


            var oldContext = _context;
            _context = _context.DefineFunction(n.Name, types, MapType(n.Symbol.ReturnType)) ??
                       throw new InvalidOperationException($"Cannot define function in {_context}");
            foreach (var node in n.Body)
                Visit(node);

            if (n.ReturnType == "void" && n.Body.Last() is not ReturnNode)
                _context.GetILGenerator().Emit(OpCodes.Ret);

            _context = oldContext;
            if (_context is ModuleContext)
                _context.CompleteFunctions();

            return null!;
        }

        public override object VisitReturnNode(ReturnNode n)
        {
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