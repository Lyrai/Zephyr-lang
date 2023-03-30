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
using Zephyr.SemanticAnalysis.Symbols;
using Zephyr.SyntaxAnalysis.ASTNodes;
using PrimitiveTypeCode = Microsoft.Cci.PrimitiveTypeCode;
using TypeSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol;

namespace Zephyr.Compiling.Roslyn;

internal class MethodCompiler: INodeVisitor<object>
{
    private ILBuilder _builder;
    private FuncDeclNode _method;
    private PEModuleBuilder _moduleBuilder;
    private MethodSymbol _symbol;
    private Dictionary<string, LocalDefinition> _locals = new();
    private Dictionary<string, int> _args = new();
    private bool _needImplicitReturn;

    public MethodCompiler(ILBuilder builder, FuncDeclNode method, PEModuleBuilder moduleBuilder, MethodSymbol symbol)
    {
        _builder = builder;
        _method = method;
        _moduleBuilder = moduleBuilder;
        _symbol = symbol;
        _needImplicitReturn = method.ReturnType == "void" && method.Body.Last() is not ReturnNode;
    }

    public MethodBody Compile()
    {
        Visit(_method);
        _builder.Realize();
        _builder.FreeBasicBlocks();
        return new MethodBody(
            _builder.RealizedIL,
            _builder.MaxStack,
            _symbol.GetCciAdapter(),
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
            _symbol is SynthesizedPrimaryConstructor
        );
    }
    
    public object VisitClassNode(ClassNode n)
    {
        return null!;
    }

    public object VisitGetNode(GetNode n)
    {
        Visit(n.Obj);
        if (!n.IsLhs)
        {
            EmitFieldOpCode(ILOpCode.Ldfld, n);
        }
        return null!;
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
                if (n.Left.TypeSymbol.Name == "string" && n.Right.TypeSymbol.Name == "string")
                {
                    EmitCall("System.String", "Concat", "String", "String");
                    break;
                }
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

                if (n.Left is GetNode)
                {
                    EmitFieldOpCode(ILOpCode.Stfld, n.Left as GetNode);
                }
                else
                {
                    StoreLocal(n.Left.Token.Value.ToString());
                }
                
                break;
            case "==":
                _builder.EmitOpCode(ILOpCode.Ceq);
                break;
            case "!=":
                _builder.EmitOpCode(ILOpCode.Ceq);
                _builder.EmitIntConstant(0);
                _builder.EmitOpCode(ILOpCode.Ceq);
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
                EmitCall("System.Console", "WriteLine", n.Operand.TypeSymbol.GetNetTypeName());
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
        Visit(n.Condition);
        var falseLabel = new object();
        var exitLabel = new object();
        _builder.EmitBranch(ILOpCode.Brfalse, falseLabel);
        Visit(n.ThenBlock);
        
        if (n.ElseBlock is not null)
        {
            _builder.EmitBranch(ILOpCode.Br, exitLabel);
        }
        
        _builder.MarkLabel(falseLabel);
        
        if (n.ElseBlock is not null)
        {
            Visit(n.ElseBlock);
            _builder.MarkLabel(exitLabel);
        }
        
        return null!;
    }

    public object VisitWhileNode(WhileNode n)
    {
        var condLabel = new object();
        var exitLabel = new object();
        
        _builder.MarkLabel(condLabel);
        
        Visit(n.Condition);
        _builder.EmitBranch(ILOpCode.Brfalse, exitLabel);

        Visit(n.Body);
        _builder.EmitBranch(ILOpCode.Br, condLabel);
        
        _builder.MarkLabel(exitLabel);
        
        return null!;
    }

    public object VisitVarNode(VarNode n)
    {
        if (n.IsLhs)
        {
            return null!;
        }

        if (_locals.ContainsKey(n.Name))
        {
            _builder.EmitLocalLoad(_locals[n.Name]);
            return null!;
        }

        _builder.EmitLoadArgumentOpcode(_args[n.Name]);
        
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
        foreach (var argument in n.Arguments)
        {
            Visit(argument);
        }

        var symbol = ResolveMethod(n);

        if (symbol.IsConstructor())
        {
            _builder.EmitOpCode(ILOpCode.Newobj, GetNewobjStackAdjustment(n.Arguments.Count));
            EmitToken(GetToken(ResolveMethod(n)));
            return null!;
        }
        
        if (symbol.RequiresInstanceReceiver)
        {
            _builder.EmitLocalLoad(_locals[(n.Callee as GetNode).Obj.Token.Value.ToString()]);
        }
        
        EmitCall(symbol, n.Arguments.Count);
        return null!;
    }

    public object VisitFuncDeclNode(FuncDeclNode n)
    {
        foreach (var parameter in n.Parameters)
        {
            _args.Add((parameter as VarDeclNode).Variable.Name, _args.Count);
        }

        if (n.IsEmpty() && n.Name == ".ctor")
        {
            SynthesizeConstructor();
            return null!;
        }
        
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
            .GetTypeByMetadataName(qualifiedClassName, true, true, out _) as TypeSymbol;
    }
    
    private TypeSymbol ResolveType(string name)
    {
        return _moduleBuilder
            .SourceModule
            .GlobalNamespace
            .GetMembers(name)
            .First() as TypeSymbol;
    }

    private MethodSymbol ResolveNetMethod(string qualifiedClassName, string methodName, params string[] paramsTypes)
    {
        return ResolveNetType(qualifiedClassName)
            .GetMembers(methodName)
            .First(symbol =>
            {
                var parameters = symbol.GetParameters();
                return parameters.Length == paramsTypes.Length && parameters.Select(x => x.Type.Name).SequenceEqual(paramsTypes);
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
    
    private IFieldReference GetToken(FieldSymbol type)
    {
        return _moduleBuilder.Translate(type, null, DiagnosticBag.GetInstance());
    }

    private int GetStackAdjustment(MethodSymbol symbol, int argCount)
    {
        var stackAdjustment = 0;
        if (!symbol.ReturnsVoid)
            stackAdjustment++;
        
        if (symbol.RequiresInstanceReceiver)
            stackAdjustment--;
        
        stackAdjustment -= argCount;

        return stackAdjustment;
    }

    private int GetNewobjStackAdjustment(int argCount)
    {
        return 1 - argCount;
    }

    private MethodSymbol ResolveMethod(FuncCallNode n)
    {
        string className;
        string methodName;
        if (n.Callable is ClassSymbol classSymbol)
        {
            className = classSymbol.Name;
            methodName = ".ctor";
        }
        else if (n.Callee is GetNode node)
        {
            className = node.Obj.TypeSymbol.Name;
            methodName = n.Name;
        }
        else
        {
            className = "<global class>";
            methodName = n.Name;
        }
        
        var parameterTypes = n.Arguments.Select(x => x.TypeSymbol.GetNetTypeName());
        var foundTypes = _moduleBuilder
            .SourceModule
            .GlobalNamespace
            .GetMembers(className)
            .OfType<NamedTypeSymbol>();
        
        if (!foundTypes.Any())
        {
            return ResolveNetMethod(className, methodName, parameterTypes.ToArray());
        }
        
        var symbol = foundTypes
            .First()
            .GetMembers(methodName)
            .OfType<MethodSymbol>()
            .First(x => x.GetParameters().Select(x1 => x1.Type.Name).SequenceEqual(parameterTypes));

        return symbol;
    }

    private FieldSymbol ResolveField(string className, string fieldName)
    {
        var type = ResolveType(className);
        return type.GetMembers(fieldName)[0] as FieldSymbol;
    }

    private void SynthesizeConstructor()
    {
        _builder.EmitOpCode(ILOpCode.Ldarg_0);
        EmitCall("System.Object", ".ctor");
        _builder.EmitRet(true);
    }

    private void EmitCall(MethodSymbol symbol, int argCount)
    {
        _builder.EmitOpCode(ILOpCode.Call, GetStackAdjustment(symbol, argCount));
        EmitToken(GetToken(symbol));
    }

    private void EmitCall(string qualifiedClassName, string methodName, params string[] argTypeNames)
    {
        var symbol = ResolveNetMethod(qualifiedClassName, methodName, argTypeNames);
        EmitCall(symbol, argTypeNames.Length);
    }

    private void EmitToken(IReference token)
    {
        _builder.EmitToken(token, null, DiagnosticBag.GetInstance());
    }
    
    private void EmitToken(ISignature token)
    {
        _builder.EmitToken(token, null, DiagnosticBag.GetInstance());
    }

    private void EmitCall(FuncCallNode n)
    {
        var symbol = ResolveMethod(n);
        EmitCall(symbol, n.Arguments.Count);
    }

    private void StoreLocal(string name)
    {
        _builder.EmitLocalStore(_locals[name]);
    }

    private void EmitFieldOpCode(ILOpCode opcode, GetNode n)
    {
        _builder.EmitOpCode(opcode);
        EmitToken(GetToken(ResolveField(n.Obj.TypeSymbol.Name, n.Token.Value.ToString())));
    }
}
