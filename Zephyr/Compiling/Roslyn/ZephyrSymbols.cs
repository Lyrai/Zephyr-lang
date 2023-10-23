using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;
using Zephyr.SyntaxAnalysis.ASTNodes;
using BindingDiagnosticBag = Microsoft.CodeAnalysis.CSharp.BindingDiagnosticBag;
using FieldSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol;
using NamedTypeSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol;
using ParameterSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol;
using Symbol = Microsoft.CodeAnalysis.CSharp.Symbol;
using TypeParameterSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.TypeParameterSymbol;
using TypeSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol;

namespace Zephyr.Compiling.Roslyn;

class ZephyrMethodSymbol : SourceMemberMethodSymbol
{
    public ZephyrMethodSymbol(NamedTypeSymbol containingType, bool isIterator, TypeSymbol returnType, FuncDeclNode n) : base(containingType, null, null, isIterator)
    {
        _parameterSymbols = ImmutableArray<ParameterSymbol>.Empty;
        _returnType = TypeWithAnnotations.Create(returnType);
        _sortKey = new LexicalSortKey(0, n.Token.Line);
        Name = n.Name;
    }

    internal override LexicalSortKey GetLexicalSortKey()
    {
        return _sortKey;
    }

    public override Accessibility DeclaredAccessibility { get => Accessibility.Public; }
    public override string Name { get; }

    public override bool IsVararg { get => false; }
    public override RefKind RefKind { get => RefKind.None; }
    public override TypeWithAnnotations ReturnTypeWithAnnotations { get => _returnType; }
    public override ImmutableArray<TypeParameterSymbol> TypeParameters { get => ImmutableArray<TypeParameterSymbol>.Empty; }
    public override ImmutableArray<ParameterSymbol> Parameters { get => _parameterSymbols; }
    internal override bool GenerateDebugInfo { get => false; }

    private ImmutableArray<ParameterSymbol> _parameterSymbols;
    private TypeWithAnnotations _returnType;
    
    public override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
    {
        return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
    }

    public override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
    {
        return ImmutableArray<TypeParameterConstraintKind>.Empty;
    }

    protected override void MethodChecks(BindingDiagnosticBag diagnostics)
    { }

    internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false)
    {
        return null;
    }

    public void SetParameters(ImmutableArray<ParameterSymbol> parameters)
    {
        _parameterSymbols = parameters;
    }

    public void SetStatic()
    {
        DeclarationModifiers |= DeclarationModifiers.Static;
    }

    internal override bool IsExpressionBodied { get => false; }

    private LexicalSortKey _sortKey;
}

class ZephyrParameterSymbol : ParameterSymbol
{
    public override Symbol ContainingSymbol { get; }
    public override ImmutableArray<Location> Locations { get => ImmutableArray<Location>.Empty; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get => ImmutableArray<SyntaxReference>.Empty; }
    public override TypeWithAnnotations TypeWithAnnotations { get; }
    public override RefKind RefKind { get => RefKind.None; }
    public override bool IsDiscard { get => false; }
    public override ImmutableArray<CustomModifier> RefCustomModifiers { get => ImmutableArray<CustomModifier>.Empty; }
    internal override MarshalPseudoCustomAttributeData? MarshallingInformation { get => null; }
    public override int Ordinal { get; }
    public override bool IsParams { get => false; }
    internal override bool IsMetadataOptional { get => false; }
    internal override bool IsMetadataIn { get => false; }
    internal override bool IsMetadataOut { get => false; }
    internal override ConstantValue? ExplicitDefaultConstantValue { get => null; }
    internal override bool IsIDispatchConstant { get => false; }
    internal override bool IsIUnknownConstant { get => false; }
    internal override bool IsCallerFilePath { get => false; }
    internal override bool IsCallerLineNumber { get => false; }
    internal override bool IsCallerMemberName { get => false; }
    internal override int CallerArgumentExpressionParameterIndex { get => Ordinal; }
    internal override FlowAnalysisAnnotations FlowAnalysisAnnotations { get => FlowAnalysisAnnotations.None; }
    internal override ImmutableHashSet<string> NotNullIfParameterNotNull { get => ImmutableHashSet<string>.Empty; }
    internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes { get => ImmutableArray<int>.Empty; }
    internal override bool HasInterpolatedStringHandlerArgumentError { get => false; }
    internal override ScopedKind EffectiveScope { get => ScopedKind.None; }
    internal override bool HasUnscopedRefAttribute { get => false; }
    internal override bool UseUpdatedEscapeRules { get => false; }

    public ZephyrParameterSymbol(Symbol containingSymbol, int ordinal, TypeSymbol type)
    {
        ContainingSymbol = containingSymbol;
        Ordinal = ordinal;
        TypeWithAnnotations = TypeWithAnnotations.Create(type);
    }
}

class ZephyrFieldSymbol : SourceFieldSymbol
{
    public ZephyrFieldSymbol(SourceMemberContainerTypeSymbol containingType, string name, TypeSymbol type) : base(containingType)
    {
        Name = name;
        _type = type;
    }

    public override string Name { get; }
    public override ImmutableArray<Location> Locations { get => ImmutableArray<Location>.Empty; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get => ImmutableArray<SyntaxReference>.Empty; }
    public override RefKind RefKind { get => RefKind.None; }
    internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
    {
        return TypeWithAnnotations.Create(_type);
    }

    public override Symbol AssociatedSymbol { get; }
    internal override ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes)
    {
        return null;
    }

    internal override Location ErrorLocation { get => null; }
    protected override DeclarationModifiers Modifiers { get => DeclarationModifiers.Public; }
    protected override SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList { get => new(); }
    private TypeSymbol _type;
}

static class AssemblySymbolExtensions
{
    public static NamedTypeSymbol GetType(this AssemblySymbol assembly, Zephyr.SemanticAnalysis.Symbols.TypeSymbol symbol)
    {
        return assembly.GetTypeByMetadataName(symbol.GetNetFullName(), true, true, out _);
    }
}
