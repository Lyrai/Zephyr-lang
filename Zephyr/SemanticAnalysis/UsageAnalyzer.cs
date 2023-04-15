// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.SemanticAnalysis;

public class UsageAnalyzer: INodeVisitor<object>
{
    public void Analyze(Node tree)
    {
        Visit(tree);
    }
    
    public object VisitClassNode(ClassNode n)
    {
        foreach (var node in n.Body)
        {
            Visit(node);
        }

        return null!;
    }

    public object VisitGetNode(GetNode n)
    {
        Visit(n.Obj);
        return null!;
    }

    public object VisitCompoundNode(CompoundNode n)
    {
        foreach (var child in n.GetChildren())
        {
            if(child is IExpression expr)
            {
                expr.SetIsStatement(true);
                expr.SetUsed(expr == n.GetChildren().Last());
            }
            
            Visit(child);
        }
        
        n.SetIsStatement(true);
        n.SetUsed(false);

        return null!;
    }

    public object VisitBinOpNode(BinOpNode n)
    {
        Visit(n.Left);
        Visit(n.Right);
        if (n.Token.Value.ToString() == "=")
        {
            var expr = n.Right as IExpression;
            expr.SetIsStatement(false);
            expr.SetUsed(true);
            n.SetIsStatement(true);
            n.SetUsed(true);
        }
        
        
        
        return null!;
    }

    public object VisitUnOpNode(UnOpNode n)
    {
        Visit(n.Operand);
        return null!;
    }

    public object VisitLiteralNode(LiteralNode n)
    {
        return null!;
    }

    public object VisitIfNode(IfNode n)
    {
        Visit(n.Condition);
        var condition = n.Condition as IExpression;
        condition.SetIsStatement(true);
        condition.SetUsed(true);
        
        Visit(n.ThenBlock);
        var expr = n.ThenBlock as IExpression;
        expr.SetUsed(true);
        expr.SetIsStatement(true);
        if (n.ElseBlock is not null)
        {
            Visit(n.ElseBlock);
            expr = n.ElseBlock as IExpression;
            expr.SetUsed(true);
            expr.SetIsStatement(true);
        }

        return null!;
    }

    public object VisitWhileNode(WhileNode n)
    {
        Visit(n.Body);
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
        throw new NotImplementedException();
    }

    public object VisitFuncCallNode(FuncCallNode n)
    {
        foreach (var expr in n.Arguments)
        {
            Visit(expr);
        }

        return null!;
    }

    public object VisitFuncDeclNode(FuncDeclNode n)
    {
        foreach (var child in n.Body)
        {
            Visit(child);
            if(child is IExpression expr)
            {
                expr.SetIsStatement(true);
                expr.SetUsed(false);
            }
        }

        return null!;
    }

    public object VisitReturnNode(ReturnNode n)
    {
        if(n.Value is not null)
        {
            Visit(n.Value);
        }

        return null!;
    }

    public object VisitNoOpNode(NoOpNode n)
    {
        return null!;
    }

    public object VisitArrayInitializerNode(ArrayInitializerNode n)
    {
        foreach (var node in n.GetChildren())
        {
            Visit(node);
        }

        return null!;
    }

    public object VisitIndexNode(IndexNode n)
    {
        Visit(n.Expression);
        Visit(n.Index);

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

    private object Visit(Node n)
    {
        return n.Accept(this);
    }
}
