using System;
using Zephyr.Compiling.Contexts;
using Zephyr.SemanticAnalysis.Symbols;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling;

public abstract class BaseCompiler: INodeVisitor<object>
{
    protected CompilationContext _context;
    
    public object VisitClassNode(ClassNode n)
    {
        return null!;
    }

    public object VisitGetNode(GetNode n)
    {
        return null!;
    }

    public object VisitCompoundNode(CompoundNode n)
    {
        return null!;
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
    
    protected Type? MapType(TypeSymbol symbol)
    {
        return MapType(symbol.Name);
    }
        
    protected Type? MapType(string name)
    {
        return name switch
        {
            "int" => typeof(int),
            "double" => typeof(double),
            "string" => typeof(string),
            "void" => typeof(void),
            "bool" => typeof(bool),
            "function" => null,
            _ => _context.GetTypeByName(name)
        };
    }
}