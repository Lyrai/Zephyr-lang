// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SemanticAnalysis;
using Zephyr.SemanticAnalysis.Symbols;
using Zephyr.SyntaxAnalysis.ASTNodes;
using ArrayTypeSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol;
using FieldSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol;
using MethodSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol;
using PrimitiveTypeCode = Microsoft.Cci.PrimitiveTypeCode;
using TypeSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol;
using ZephyrTypeSymbol = Zephyr.SemanticAnalysis.Symbols.TypeSymbol;
using ZephyrArrayTypeSymbol = Zephyr.SemanticAnalysis.Symbols.ArrayTypeSymbol;

namespace Zephyr.Compiling.Roslyn;

internal class MethodCompiler: INodeVisitor<object>
{
    private readonly ILBuilder _builder;
    private readonly FuncDeclNode _method;
    private readonly PEModuleBuilder _moduleBuilder;
    private readonly MethodSymbol _symbol;
    private readonly Dictionary<string, LocalDefinition> _locals = new();
    private readonly Dictionary<string, int> _args = new();
    private readonly Dictionary<string, ZephyrTypeSymbol> _predefinedTypes = ScopedSymbolTable.GetPredefinedTypes();
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
            if (child is not IExpression expr)
            {
                Visit(child);
                continue;
            }
            
            if (expr is { ReturnsValue: true, IsUsed: false, CanBeDropped: true})
            {
                continue;
            }

            VisitWithStackGuard(child);
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
                if (n.Left.TypeSymbol.Name != "string")
                {
                    _builder.EmitOpCode(ILOpCode.Add);
                    break;
                }
                
                if (n.Right.TypeSymbol.Name != "string")
                {
                    if(n.Left is LiteralNode)
                    {
                        var tempName = "tmp" + _locals.Count;
                        var varNode = new VarNode(new Token(TokenType.Id, tempName, 0, 0), true);
                        var declNode = new VarDeclNode(varNode, null);
                        declNode.SetType(n.Right.TypeSymbol);
                        Visit(declNode);
                        StoreLocal(tempName);
                        _builder.EmitLoadAddress(_locals[tempName]);
                    }
                    
                    EmitCall(n.Right.TypeSymbol, "ToString");
                }
                
                EmitCall(n.Left.TypeSymbol, "Concat", "String", "String");
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

                if (n.Left is GetNode node)
                {
                    EmitFieldOpCode(ILOpCode.Stfld, node);
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
            case ">":
                _builder.EmitOpCode(ILOpCode.Cgt);
                break;
            case "<":
                _builder.EmitOpCode(ILOpCode.Clt);
                break;
            case ">=":
                _builder.EmitOpCode(ILOpCode.Clt);
                _builder.EmitIntConstant(0);
                _builder.EmitOpCode(ILOpCode.Ceq);
                break;
            case "<=":
                _builder.EmitOpCode(ILOpCode.Cgt);
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
                EmitCall(_predefinedTypes["System.Console"], "WriteLine", n.Operand.TypeSymbol.GetNetName());
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
        VisitWithStackGuard(n.ThenBlock);
        
        if (n.ElseBlock is not null)
        {
            _builder.EmitBranch(ILOpCode.Br, exitLabel);
        }
        
        _builder.MarkLabel(falseLabel);
        
        if (n.ElseBlock is not null)
        {
            VisitWithStackGuard(n.ElseBlock);
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

        VisitWithStackGuard(n.Body);
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

        EmitLoadLocal(n.Name);
        
        return null!;
    }

    public object VisitVarDeclNode(VarDeclNode n)
    {
        var token = GetToken(ResolveType(n));
        var definition = _builder.LocalSlotManager.AllocateSlot(token, LocalSlotConstraints.None);
        /*var definition = _builder.LocalSlotManager.DeclareLocal(
            token,
            null,
            n.Variable.Name,
            SynthesizedLocalKind.UserDefined,
            LocalDebugId.None,
            LocalVariableAttributes.None,
            LocalSlotConstraints.None,
            default,
            default,
            false
        );*/
        _locals.Add(n.Variable.Name, definition);
        
        return null!;
    }

    public object VisitPropertyDeclNode(PropertyDeclNode n)
    {
        throw new NotImplementedException();
    }

    public object VisitFuncCallNode(FuncCallNode n)
    {
        var symbol = ResolveMethod(n);

        if (symbol.IsConstructor())
        {
            _builder.EmitOpCode(ILOpCode.Newobj, GetNewobjStackAdjustment(n.Arguments.Count));
            EmitToken(GetToken(ResolveMethod(n)));
            return null!;
        }
        
        if (symbol.RequiresInstanceReceiver)
        {
            var callee = n.Callee as GetNode;
            var receiver = callee.Obj;
            var name = receiver.Token.Value.ToString();
            
            if (receiver.TypeSymbol.IsValueType())
            {
                _builder.EmitLoadAddress(_locals[name]);
            }
            else
            {
                EmitLoadLocal(name);
            }
            
        }
        
        foreach (var argument in n.Arguments)
        {
            Visit(argument);
        }
        
        EmitCall(symbol, n.Arguments.Count);
        return null!;
    }

    public object VisitFuncDeclNode(FuncDeclNode n)
    {
        if(!n.IsStatic)
        {
            _args.Add("this", 0);
        }
        
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
            if (node is not IExpression expr)
            {
                Visit(node);
                continue;
            }
            
            if (expr is { ReturnsValue: true, IsUsed: false, CanBeDropped: true})
            {
                continue;
            }

            VisitWithStackGuard(node);
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

    public object VisitArrayInitializerNode(ArrayInitializerNode n)
    {
        var arrayTypeSymbol = n.TypeSymbol as ZephyrArrayTypeSymbol;
        var elementType = GetToken(ResolveType(arrayTypeSymbol.ElementType));
        
        _builder.EmitIntConstant(n.ElementsCount);
        _builder.EmitOpCode(ILOpCode.Newarr);
        EmitToken(elementType);
        var f = new[] { 1, 2 };

        if (n.ElementsCount <= 0)
        {
            return null!;
        }

        var elements = n.GetChildren();
        for (var i = 0; i < n.ElementsCount; i++)
        {
            _builder.EmitOpCode(ILOpCode.Dup);
            _builder.EmitIntConstant(i);
            Visit(elements[i]);
            EmitStoreElement(arrayTypeSymbol.ElementType);
        }

        return null!;
    }

    public object VisitIndexNode(IndexNode n)
    {
        Visit(n.Expression);
        Visit(n.Index);

        if (!n.IsLhs)
        {
            EmitLoadElement((n.Expression.TypeSymbol as ZephyrArrayTypeSymbol).ElementType);
        }

        return null!;
    }

    private object Visit(Node n)
    {
        return n.Accept(this);
    }

    private TypeSymbol ResolveType(Node n)
    {
        var type = n.TypeSymbol;
        if(!type.IsArray)
        {
            return ResolveType(type);
        }

        var arrayTypeSymbol = n.TypeSymbol as ZephyrArrayTypeSymbol;
        var elementType = ResolveType(arrayTypeSymbol.ElementType);
        var arrayType = TypeWithAnnotations.Create(false, elementType);
        return ArrayTypeSymbol.CreateCSharpArray(_moduleBuilder.Compilation.Assembly, arrayType);
    }

    private TypeSymbol ResolveType(ZephyrTypeSymbol type)
    {
        return _moduleBuilder
            .Compilation
            .Assembly
            .GetTypeByMetadataName(type.GetNetFullName(), true, true, out _) as TypeSymbol;
    }

    private MethodSymbol ResolveMethod(ZephyrTypeSymbol type, string methodName, params string[] paramsTypes)
    {
        return ResolveType(type)
            .GetMembers(methodName)
            .First(symbol =>
            {
                var parameters = symbol.GetParameters();
                return parameters.Select(x => x.Type.Name).SequenceEqual(paramsTypes);
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
        ZephyrTypeSymbol type;
        string methodName;
        if (n.Callable is ClassSymbol)
        {
            type = n.ReturnType;
            methodName = ".ctor";
        }
        else if (n.Callee is GetNode node)
        {
            type = node.Obj.TypeSymbol;
            methodName = n.Name;
        }
        else
        {
            type = _predefinedTypes["<global class>"];
            methodName = n.Name;
        }

        var parameterTypes = n.Arguments.Select(x => x.TypeSymbol.GetNetName());

        return ResolveMethod(type, methodName, parameterTypes.ToArray());
    }

    private FieldSymbol ResolveField(ZephyrTypeSymbol type, string fieldName)
    {
        var netType = ResolveType(type);
        return netType.GetMembers(fieldName)[0] as FieldSymbol;
    }

    private void SynthesizeConstructor()
    {
        _builder.EmitOpCode(ILOpCode.Ldarg_0);
        EmitCall(_predefinedTypes["System.Object"], ".ctor");
        _builder.EmitRet(true);
    }

    private void EmitCall(MethodSymbol symbol, int argCount)
    {
        var opcode = symbol.IsVirtual || symbol.IsOverride || symbol.IsAbstract ? ILOpCode.Callvirt : ILOpCode.Call;
        _builder.EmitOpCode(opcode, GetStackAdjustment(symbol, argCount));
        EmitToken(GetToken(symbol));
    }

    private void EmitCall(ZephyrTypeSymbol type, string methodName, params string[] argTypeNames)
    {
        var symbol = ResolveMethod(type, methodName, argTypeNames);
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
        EmitToken(GetToken(ResolveField(n.Obj.TypeSymbol, n.Token.Value.ToString())));
    }

    private void VisitWithStackGuard(Node node)
    {
        var expr = node as IExpression;
        Visit(node);
        if (expr is { ReturnsValue: true, IsUsed: false })
        {
            _builder.EmitOpCode(ILOpCode.Pop);
        }
    }

    private void EmitLoadLocal(string name)
    {
        if (_locals.TryGetValue(name, out var local))
        {
            _builder.EmitLocalLoad(local);
        }
        else
        {
            _builder.EmitLoadArgumentOpcode(_args[name]);
        }
    }

    private void EmitStoreElement(ZephyrTypeSymbol symbol)
    {
        var typeCode = symbol.GetTypeCode();
        switch (typeCode)
        {
            case PrimitiveTypeCode.Boolean:
                _builder.EmitOpCode(ILOpCode.Stelem_i1);
                break;
            case PrimitiveTypeCode.Int32:
                _builder.EmitOpCode(ILOpCode.Stelem_i4);
                break;
            case PrimitiveTypeCode.Float64:
                _builder.EmitOpCode(ILOpCode.Stelem_r8);
                break;
            default:
                _builder.EmitOpCode(ILOpCode.Stelem_ref);
                break;
        }
    }
    
    private void EmitLoadElement(ZephyrTypeSymbol symbol)
    {
        var typeCode = symbol.GetTypeCode();
        switch (typeCode)
        {
            case PrimitiveTypeCode.Boolean:
                _builder.EmitOpCode(ILOpCode.Ldelem_i1);
                break;
            case PrimitiveTypeCode.Int32:
                _builder.EmitOpCode(ILOpCode.Ldelem_i4);
                break;
            case PrimitiveTypeCode.Float64:
                _builder.EmitOpCode(ILOpCode.Ldelem_r8);
                break;
            default:
                _builder.EmitOpCode(ILOpCode.Ldelem_ref);
                break;
        }
    }
}
