using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling
{
    public class Compiler: INodeVisitor<object>
    {
        public object VisitClassNode(ClassNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitGetNode(GetNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitCompoundNode(CompoundNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitBinOpNode(BinOpNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitUnOpNode(UnOpNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitLiteralNode(LiteralNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitIfNode(IfNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitWhileNode(WhileNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitVarNode(VarNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitVarDeclNode(VarDeclNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitPropertyDeclNode(PropertyDeclNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitFuncCallNode(FuncCallNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitFuncDeclNode(FuncDeclNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitReturnNode(ReturnNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitNoOpNode(NoOpNode n)
        {
            throw new System.NotImplementedException();
        }
    }
}