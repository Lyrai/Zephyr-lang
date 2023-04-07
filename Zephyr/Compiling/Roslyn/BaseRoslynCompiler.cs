// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling.Roslyn;

internal abstract class BaseRoslynCompiler<T>: INodeVisitor<T>
{
    public CSharpCompilation Compilation => _compilation;
    protected CSharpCompilation _compilation;
    protected const string GlobalClassName = "<global class>";
    protected const string EntryPointName = "main";
    protected const string QualifiedNameSeparator = "::";
    protected const string EntryPointQualifiedName = GlobalClassName + QualifiedNameSeparator + EntryPointName;
    protected bool _compilationFinished = false;

    public virtual void CompilationFinished()
    {
        Debug.Assert(!_compilationFinished);
        _compilationFinished = true;
    }

    protected virtual void RestartCompiler()
    {
        _compilationFinished = false;
    }

    public virtual T VisitClassNode(ClassNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitGetNode(GetNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitCompoundNode(CompoundNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitBinOpNode(BinOpNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitUnOpNode(UnOpNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitLiteralNode(LiteralNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitIfNode(IfNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitWhileNode(WhileNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitVarNode(VarNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitVarDeclNode(VarDeclNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitPropertyDeclNode(PropertyDeclNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitFuncCallNode(FuncCallNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitFuncDeclNode(FuncDeclNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitReturnNode(ReturnNode n)
    {
        throw new NotImplementedException();
    }

    public virtual T VisitNoOpNode(NoOpNode n)
    {
        throw new NotImplementedException();
    }

    public T VisitArrayInitializerNode(ArrayInitializerNode n)
    {
        throw new NotImplementedException();
    }

    protected T Visit(Node n)
    {
        return n.Accept(this);
    }
    
    protected Symbol GetSymbol(PEModuleBuilder moduleBuilder, string className, string memberName)
    {
        var classSymbol = moduleBuilder
            .SourceModule
            .GlobalNamespace
            .GetMembers(className)
            .Cast<SourceNamedTypeSymbol>()
            .First();
        var methodSymbol = classSymbol.GetMembers(memberName)[0];
        return methodSymbol;
    }
}
