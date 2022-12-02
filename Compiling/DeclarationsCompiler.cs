using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Zephyr.Compiling.Contexts;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling;

public class DeclarationsCompiler: BaseCompiler
{
    private Node _tree;
    
    public DeclarationsCompiler(Node tree)
    {
        _tree = tree;
        _context = _context = new AssemblyContext("Main", null);
    }

    public CompilationContext Compile()
    {
        _context = _context.DefineModule("Main") ?? throw new InvalidOperationException("Could not define module \"Main\"");
        Visit(_tree);
        _context.CompleteFunctions();
        return _context;
    }
    
    public object VisitClassNode(ClassNode n)
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

    public object VisitGetNode(GetNode n)
    {
        return null!;
    }

    public object VisitCompoundNode(CompoundNode n)
    {
        throw new System.NotImplementedException();
    }

    public object VisitBinOpNode(BinOpNode n)
    {
        return null!;
    }

    public object VisitUnOpNode(UnOpNode n)
    {
        return null!;
    }

    public object VisitLiteralNode(LiteralNode n)
    {
        return null!;
    }

    public object VisitIfNode(IfNode n)
    {
        return null!;
    }

    public object VisitWhileNode(WhileNode n)
    {
        return null!;
    }

    public object VisitVarNode(VarNode n)
    {
        return null!;
    }

    public object VisitVarDeclNode(VarDeclNode n)
    {
        return null!;
    }

    public object VisitPropertyDeclNode(PropertyDeclNode n)
    {
        return null!;
    }

    public object VisitFuncCallNode(FuncCallNode n)
    {
        return null!;
    }

    public object VisitFuncDeclNode(FuncDeclNode n)
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
            
        return null!;
    }

    public object VisitReturnNode(ReturnNode n)
    {
        return null!;
    }

    public object VisitNoOpNode(NoOpNode n)
    {
        return null!;
    }

    private object Visit(Node n)
    {
        return n.Accept(this);
    }
}