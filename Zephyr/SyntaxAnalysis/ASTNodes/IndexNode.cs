// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Zephyr.SemanticAnalysis;

namespace Zephyr.SyntaxAnalysis.ASTNodes;

public class IndexNode: Node, IExpression
{
    public bool IsStatement { get; private set; }
    public bool IsUsed { get; private set; }
    public bool ReturnsValue => true;
    public bool CanBeDropped => true;
    public Node Expression { get; private set; }
    public Node Index { get; private set; }

    public IndexNode(Node expression, Node index)
    {
        Expression = expression;
        Index = index;
    }
    
    public override T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitIndexNode(this);
    }

    public void SetIsStatement(bool isStatement)
    {
        IsStatement = isStatement;
    }

    public void SetUsed(bool used)
    {
        IsUsed = used;
    }
}
