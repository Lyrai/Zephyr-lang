using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr
{
    public interface INodeVisitor<T>
    {
        T VisitClassNode(ClassNode n);
        T VisitGetNode(GetNode n);
        T VisitCompoundNode(CompoundNode n);
        T VisitBinOpNode(BinOpNode n);
        T VisitUnOpNode(UnOpNode n);
        T VisitLiteralNode(LiteralNode n);
        T VisitIfNode(IfNode n);
        T VisitWhileNode(WhileNode n);
        T VisitVarNode(VarNode n);
        T VisitVarDeclNode(VarDeclNode n);
        T VisitPropertyDeclNode(PropertyDeclNode n);
        T VisitFuncCallNode(FuncCallNode n);
        T VisitFuncDeclNode(FuncDeclNode n);
        T VisitReturnNode(ReturnNode n);
        T VisitNoOpNode(NoOpNode n);
        T VisitArrayInitializerNode(ArrayInitializerNode n);
        T VisitIndexNode(IndexNode n);
    }
}
