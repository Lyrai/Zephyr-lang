// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace Zephyr.SemanticAnalysis.Symbols;

public class ArrayTypeSymbol: TypeSymbol
{
    public TypeSymbol ElementType { get; private set; }

    public ArrayTypeSymbol(string name, TypeSymbol elementType) : base(name)
    {
        ElementType = elementType;
    }
    
    public string GetElementTypeFullName()
    {
        return ElementType.GetNetFullTypeName() ?? ElementType.Name;
    }
    
    public string GetElementTypeName()
    {
        return ElementType.GetNetTypeName() ?? ElementType.Name;
    }

    public override Type? GetNetType()
    {
        return ElementType.GetNetType().MakeArrayType();
    }
}
