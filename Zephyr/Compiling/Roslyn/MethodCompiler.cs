// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Zephyr.SyntaxAnalysis.ASTNodes;
using PrimitiveTypeCode = Microsoft.Cci.PrimitiveTypeCode;

namespace Zephyr.Compiling.Roslyn;

internal class MethodCompiler: INodeVisitor<object>
{
    private ILBuilder _builder;
    private FuncDeclNode _method;
    private PEModuleBuilder _moduleBuilder;
    private Dictionary<string, LocalDefinition> _locals = new();
    private bool _needImplicitReturn;

    public MethodCompiler(ILBuilder builder, FuncDeclNode method, PEModuleBuilder moduleBuidler)
    {
        _builder = builder;
        _method = method;
        _moduleBuilder = moduleBuidler;
        _needImplicitReturn = method.ReturnType == "void" && method.Body.Last() is not ReturnNode;
    }

    public MethodBody Compile(MethodSymbol method)
    {
        Visit(_method);
        _builder.Realize();
        _builder.FreeBasicBlocks();
        return new MethodBody(
            _builder.RealizedIL,
            _builder.MaxStack,
            method.GetCciAdapter(),
            new DebugId(0, _moduleBuilder.CurrentGenerationOrdinal),
            _builder.LocalSlotManager.LocalsInOrder(),
            _builder.RealizedSequencePoints,
            null,
            _builder.RealizedExceptionHandlers,
            _builder.AreLocalsZeroed,
            false,
            _builder.GetAllScopes(),
            _builder.HasDynamicLocal,
            null,
            ImmutableArray<LambdaDebugInfo>.Empty,
            ImmutableArray<ClosureDebugInfo>.Empty,
            null,
            default,
            default,
            default,
            StateMachineStatesDebugInfo.Create(null, ImmutableArray<StateMachineStateDebugInfo>.Empty),
            null,
            ImmutableArray<SourceSpan>.Empty,
            method is SynthesizedPrimaryConstructor
        );
    }
    
    public object VisitClassNode(ClassNode n)
    {
        throw new NotImplementedException();
    }

    public object VisitGetNode(GetNode n)
    {
        throw new NotImplementedException();
    }

    public object VisitCompoundNode(CompoundNode n)
    {
        foreach (var child in n.GetChildren())
        {
            Visit(child);
        }

        return null!;
    }

    public object VisitBinOpNode(BinOpNode n)
    {
        Visit(n.Left);
        Visit(n.Right);
        
        switch (n.Token.Value)
        {
            case "+":
                _builder.EmitOpCode(ILOpCode.Add);
                break;
            case "-":
                _builder.EmitOpCode(ILOpCode.Sub);
                break;
            case "*":
                _builder.EmitOpCode(ILOpCode.Mul);
                break;
            case "/":
                _builder.EmitOpCode(ILOpCode.Div);
                break;
            case "=":
                if (n.Left.TypeSymbol != n.Right.TypeSymbol)
                {
                    var left = n.Left.TypeSymbol.GetTypeCode();
                    var right = n.Right.TypeSymbol.GetTypeCode();
                    
                    _builder.EmitNumericConversion(right, left, true);
                }
                
                _builder.EmitLocalStore(_locals[n.Left.Token.Value.ToString()]);
                break;
            default:
                throw new InvalidOperationException($"Invalid binary operator {n.Value}");
        }

        return null!;
    }

    public object VisitUnOpNode(UnOpNode n)
    {
        Visit(n.Operand);
        
        switch (n.Token.Value.ToString())
        {
            case "print":
                _builder.EmitOpCode(ILOpCode.Call, -1);
                var method = ResolveNetMethod("System.Console", "WriteLine", n.Operand.TypeSymbol.GetNetTypeName());
                _builder.EmitToken(
                    GetToken(method),
                    null, 
                    DiagnosticBag.GetInstance()
                    );
                break;
            case "-":
                _builder.EmitOpCode(ILOpCode.Neg);
                break;
        }

        return null!;
    }

    public object VisitLiteralNode(LiteralNode n)
    {
        switch (n.TypeSymbol.Name)
        {
            case "int":
                _builder.EmitIntConstant((int)n.Value);
                break;
            case "double":
                _builder.EmitDoubleConstant((double)n.Value);
                break;
            case "string":
                _builder.EmitStringConstant((string)n.Value);
                break;
            case "bool":
                _builder.EmitBoolConstant((bool)n.Value);
                break;
            default:
                throw new ArgumentException($"Invalid literal type: {n.TypeSymbol.Name}");
        }

        return null!;
    }

    public object VisitIfNode(IfNode n)
    {
        throw new NotImplementedException();
    }

    public object VisitWhileNode(WhileNode n)
    {
        throw new NotImplementedException();
    }

    public object VisitVarNode(VarNode n)
    {
        if (!n.IsLhs)
        {
            _builder.EmitLocalLoad(_locals[n.Name]);
        }
        
        return null!;
    }

    public object VisitVarDeclNode(VarDeclNode n)
    {
        var token = GetToken(ResolveNetType(n.TypeSymbol.GetNetFullTypeName()));
        var definition = _builder.LocalSlotManager.AllocateSlot(token, LocalSlotConstraints.None);
        _locals.Add(n.Variable.Name, definition);
        
        return null!;
    }

    public object VisitPropertyDeclNode(PropertyDeclNode n)
    {
        throw new NotImplementedException();
    }

    public object VisitFuncCallNode(FuncCallNode n)
    {
        throw new NotImplementedException();
    }

    public object VisitFuncDeclNode(FuncDeclNode n)
    {
        foreach (var node in n.Body)
        {
            Visit(node);
        }

        if (_needImplicitReturn)
        {
            _builder.EmitRet(true);
        }

        return null!;
    }

    public object VisitReturnNode(ReturnNode n)
    {
        Visit(n.Value);
        _builder.EmitRet(_method.ReturnType == "void");
        if (_needImplicitReturn)
        {
            _needImplicitReturn = false;
        }
        return null!;
    }

    public object VisitNoOpNode(NoOpNode n)
    {
        return null!;
    }
    
    private object Visit(Node n)
    {
        return n.Accept(this);
    }

    private TypeSymbol ResolveNetType(string qualifiedClassName)
    {
        return _moduleBuilder
            .Compilation
            .Assembly
            .GetTypeByMetadataName(qualifiedClassName, true, true, out var _) as TypeSymbol;
    }

    private MethodSymbol ResolveNetMethod(string qualifiedClassName, string methodName, string paramType)
    {
        return _moduleBuilder
            .Compilation
            .Assembly
            .GetTypeByMetadataName(qualifiedClassName, true, true, out var _)
            .GetMembers(methodName)
            .First(symbol =>
            {
                var parameters = symbol.GetParameters();
                return parameters.Length == 1 && parameters[0].Type.Name == paramType;
            }) as MethodSymbol;
    }

    private ISignature GetToken(MethodSymbol method)
    {
        return _moduleBuilder.Translate(method, null, DiagnosticBag.GetInstance());
    }
    
    private ITypeReference GetToken(TypeSymbol type)
    {
        return _moduleBuilder.Translate(type, null, DiagnosticBag.GetInstance());
    }
}
