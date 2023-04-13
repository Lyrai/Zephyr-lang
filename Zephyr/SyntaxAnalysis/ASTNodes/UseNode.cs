// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Zephyr.SyntaxAnalysis.ASTNodes;

public class UseNode: Node
{
    public string Namespace { get; private set; }
    
    public UseNode(string @namespace)
    {
        Namespace = @namespace;
    }
    
    public override T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitUseNode(this);
    }
}
