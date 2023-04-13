// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;

namespace Zephyr.SyntaxAnalysis.ASTNodes;

public class ConversionNode: Node, IExpression
{
    public bool IsStatement { get; private set; }
    public bool IsUsed { get; private set; }
    public bool ReturnsValue => true;
    public bool CanBeDropped => Operand is IExpression { CanBeDropped: true };
    
    public TypeSymbol From { get; }
    public TypeSymbol To { get; }
    public Node Operand { get; private set; }
    
    public override TypeSymbol TypeSymbol => To;

    public ConversionNode(TypeSymbol from, TypeSymbol to, Node operand)
    {
        From = from;
        To = to;
        Operand = operand;
    }

    public override void Replace(Node oldItem, Node newItem)
    {
        Operand = newItem;
    }

    public void SetIsStatement(bool isStatement)
    {
        IsStatement = isStatement;
    }

    public void SetUsed(bool used)
    {
        IsUsed = used;
    }

    public override T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitConversionNode(this);
    }
}
