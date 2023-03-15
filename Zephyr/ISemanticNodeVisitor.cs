using Zephyr.SemanticAnalysis.SemanticNodes;

namespace Zephyr
{
    public interface ISemanticNodeVisitor<T>
    {
        //T VisitClassNode(ClassSemanticNode n);
        //T VisitGetNode(GetSemanticNode n);
        //T VisitCompoundNode(CompoundSemanticNode n);
        //T VisitBinOpNode(BinOpSemanticNode n);
        //T VisitUnOpNode(UnOpSemanticNode n);
        T VisitLiteralNode(LiteralSemanticNode n);
        //T VisitIfNode(IfSemanticNode n);
        //T VisitWhileNode(WhileSemanticNode n);
        T VisitVarNode(VarSemanticNode n);
        T VisitVarDeclNode(VarDeclSemanticNode n);
        //T VisitPropertyDeclNode(PropertyDeclSemanticNode n);
        //T VisitFuncCallNode(FuncCallSemanticNode n);
        T VisitFuncDeclNode(FuncDeclSemanticNode n);
        //T VisitReturnNode(ReturnSemanticNode n);
    }
}