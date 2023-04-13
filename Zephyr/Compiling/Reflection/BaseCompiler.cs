using Zephyr.Compiling.Contexts;
using Zephyr.SemanticAnalysis.Symbols;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling.Reflection {

public abstract class BaseCompiler: INodeVisitor<object>
{
    protected CompilationContext _context;
    
    public virtual object VisitClassNode(ClassNode n)
    {
        return null!;
    }

    public virtual object VisitGetNode(GetNode n)
    {
        return null!;
    }

    public virtual object VisitCompoundNode(CompoundNode n)
    {
        return null!;
    }

    public virtual object VisitBinOpNode(BinOpNode n)
    {
        return null!;
    }

    public virtual object VisitUnOpNode(UnOpNode n)
    {
        return null!;
    }

    public virtual object VisitLiteralNode(LiteralNode n)
    {
        return null!;
    }

    public virtual object VisitIfNode(IfNode n)
    {
        return null!;
    }

    public virtual object VisitWhileNode(WhileNode n)
    {
        return null!;
    }

    public virtual object VisitVarNode(VarNode n)
    {
        return null!;
    }

    public virtual object VisitVarDeclNode(VarDeclNode n)
    {
        return null!;
    }

    public virtual object VisitPropertyDeclNode(PropertyDeclNode n)
    {
        return null!;
    }

    public virtual object VisitFuncCallNode(FuncCallNode n)
    {
        return null!;
    }

    public virtual object VisitFuncDeclNode(FuncDeclNode n)
    {
        return null!;
    }

    public virtual object VisitReturnNode(ReturnNode n)
    {
        return null!;
    }

    public virtual object VisitNoOpNode(NoOpNode n)
    {
        return null!;
    }

    public object VisitArrayInitializerNode(ArrayInitializerNode n)
    {
        return null!;
    }

    public object VisitIndexNode(IndexNode n)
    {
        return null!;
    }

    public object VisitUseNode(UseNode n)
    {
        return null!;
    }

    public object VisitConversionNode(ConversionNode n)
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
}}
