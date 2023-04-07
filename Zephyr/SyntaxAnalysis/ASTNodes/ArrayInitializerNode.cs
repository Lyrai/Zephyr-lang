// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Zephyr.SemanticAnalysis;

namespace Zephyr.SyntaxAnalysis.ASTNodes;

public class ArrayInitializerNode: Node, IExpression
{
    public int ElementsCount => _elements.Count;
    public bool IsStatement { get; private set; }
    public bool IsUsed { get; private set; }
    public bool ReturnsValue => true;
    public bool CanBeDropped => _elements.All(elem => elem is IExpression { CanBeDropped: true });
    private List<Node> _elements;

    public ArrayInitializerNode(List<Node> elements)
    {
        _elements = elements;
    }
    
    public override T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitArrayInitializerNode(this);
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
