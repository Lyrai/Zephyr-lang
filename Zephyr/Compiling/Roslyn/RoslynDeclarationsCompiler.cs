// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SemanticAnalysis.Symbols;
using Zephyr.SyntaxAnalysis.ASTNodes;
using BindingDiagnosticBag = Microsoft.CodeAnalysis.CSharp.BindingDiagnosticBag;
using FieldSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol;
using MethodSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol;
using NamedTypeSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol;
using NamespaceOrTypeSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol;
using ParameterSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol;
using Symbol = Microsoft.CodeAnalysis.CSharp.Symbol;
using TypeParameterSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.TypeParameterSymbol;
using TypeSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol;

namespace Zephyr.Compiling.Roslyn;

internal class RoslynDeclarationsCompiler: BaseRoslynCompiler<(Declaration, Symbol)>
//internal class RoslynDeclarationsCompiler: BaseRoslynCompiler<Declaration>
{
    private readonly Dictionary<string, Node> _functions = new();
    private PEModuleBuilder _moduleBuilder;
    private readonly Stack<string> _emitContext = new();
    private Dictionary<string, ImmutableSegmentedDictionary<string, VoidResult>> _classes = new();
    private SourceNamespaceSymbol _globalNamespace;
    private Stack<TypeSymbol> _typeSymbols = new();

    public RoslynDeclarationsCompiler(string assemblyName)
    {
#if NET6_0
        _compilation = CSharpCompilation.Create(assemblyName).WithReferences(
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Console").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        );
#else
        _compilation = CSharpCompilation.Create(assemblyName).WithReferences(
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(GetAssembly("System").Location)
        );
#endif
        _compilation = _compilation.WithOptions(new CSharpCompilationOptions(
            outputKind: OutputKind.ConsoleApplication,
            optimizationLevel: OptimizationLevel.Release,
            metadataImportOptions: MetadataImportOptions.All));
    }

    public ImmutableDictionary<string, Node> Compile(Node n)
    {
        RestartCompiler();
        
        Debug.Assert(n is CompoundNode);
        var node = n as CompoundNode;
        //var globalClassMembers = ImmutableSegmentedDictionary<string, VoidResult>.Empty;
        var compilationUnit = SyntaxFactory.CompilationUnit();
        
        var globalNamespaceMembers = ImmutableArray<SingleNamespaceOrTypeDeclaration>.Empty;
        _classes[GlobalClassName] = ImmutableSegmentedDictionary<string, VoidResult>.Empty;

        foreach (var child in node.GetChildren().Where(node => node is not UseNode))
        {
            if (child is ClassNode)
            {
                //compilationUnit = compilationUnit.AddMembers(Visit(child));
                var (decl, _) = Visit(child);
                globalNamespaceMembers = globalNamespaceMembers.Add((SingleTypeDeclaration)decl);
                continue;
            }

            /*globalClass ??= SyntaxFactory
                .ClassDeclaration(GlobalClassName)
                .WithModifiers(GetKeywords(SyntaxKind.StaticKeyword));*/

            //globalClassMembers = globalClassMembers.Add(child.Token.Value.ToString(), new VoidResult());

            _emitContext.Push(GlobalClassName);
            Visit(child);
            //globalClass = globalClass.AddMembers(member);
            _emitContext.Pop();
        }

        /*if (globalClass is not null)
        {
            compilationUnit = compilationUnit.AddMembers(globalClass);
        }*/

        /*foreach (var (k, v) in _classes)
        {
            var decl = CreateClassDeclaration(k, v);
            globalNamespaceMembers = globalNamespaceMembers.Add(decl);
        }*/

        var globalClass = CreateClassDeclaration(GlobalClassName, _classes[GlobalClassName], 0);
        globalNamespaceMembers = globalNamespaceMembers.Add(globalClass);
        
        _globalNamespace = CreateNamespaceSymbol("<global namespace>", globalNamespaceMembers);
        var globalClassMembers = new Dictionary<string, ImmutableArray<Symbol>>();
        var globalClassSymbol =
            _globalNamespace.GetMembersUnordered()
                .First(x => (x as NamedTypeSymbol).Name == GlobalClassName) as SourceNamedTypeSymbol;
        foreach (var child in node.GetChildren().Where(node => node is not UseNode))
        {
            if (child is ClassNode)
            {
                Visit(child);
                continue;
            }

            _typeSymbols.Push(globalClassSymbol);
            var (_, symbol) = Visit(child);
            globalClassMembers[(child as FuncDeclNode).Name] = new List<Symbol> {symbol}.ToImmutableArray();
            _typeSymbols.Pop();
        }
        
        globalClassSymbol.SetMembersDictionary(globalClassMembers);
        /*foreach (var classDecl in globalNamespaceMembers)
        {
            var symbol = _globalNamespace
                .GetMembersUnordered()
                .First(x => (x as NamedTypeSymbol).Name == classDecl.Name) as SourceNamedTypeSymbol;
            
            var members = new Dictionary<string, ImmutableArray<Symbol>>();
            foreach (var (name, _) in _classes[classDecl.Name])
            {
                var nn = classDecl.Name + QualifiedNameSeparator + name;
                if (_functions.ContainsKey(nn))
                {
                    var s = CreateMethodSymbol(symbol, _functions[nn] as FuncDeclNode);
                    var l = ImmutableArray<Symbol>.Empty;
                    members[name] = l.Add(s);
                }
            }
            
            symbol.SetMembersDictionary(members);
        }*/

        //var globalClassSymbol = CreateClassSymbol(namespaceSymbol, globalClass);
        
        //var mainSymbol = SourceOrdinaryMethodSymbol.CreateMethodSymbol(globalClassSymbol, null, null, false, BindingDiagnosticBag.Discarded);
        /*var mainSymbol = new MethodSymbolTest(globalClassSymbol, false, globalClassSymbol);
        var members = ImmutableArray<Symbol>.Empty;
        members = members.Add(mainSymbol);
        globalClassSymbol = namespaceSymbol.GetMembers(GlobalClassName)[0] as SourceNamedTypeSymbol;
        var dict = new Dictionary<string, ImmutableArray<Symbol>> { ["main"] = members };
        globalClassSymbol.SetMembersDictionary(dict);*/

        //_compilation = _compilation.AddSyntaxTrees(CSharpSyntaxTree.Create(compilationUnit));
        (_compilation.SourceModule as SourceModuleSymbol).SetGlobalNamespace(_globalNamespace);
        _globalNamespace.GetMembers();
        //var s = _compilation.SourceModule.GlobalNamespace.GetMembers();
        //_compilation.SourceModule.GlobalNamespace;
        /*var _ = _compilation.SourceAssembly.Modules[0].GlobalNamespace;
        var _1 = MergedNamespaceDeclaration.Create(SingleNamespaceDeclaration.Create("Test1", false, false, null, null,
            ImmutableArray<SingleNamespaceOrTypeDeclaration>.Empty, ImmutableArray<Diagnostic>.Empty));
        _compilation.SymbolDeclaredEvent(_1);*/

        return _functions.ToImmutableDictionary();
    }

    protected override void RestartCompiler()
    {
        _functions.Clear();
        _emitContext.Clear();
        base.RestartCompiler();
    }

    public PEModuleBuilder GetModuleBuilder(CSharpCompilation compilation)
    {
        //Debug.Assert(_compilationFinished);
        if (_moduleBuilder is not null)
        {
            return _moduleBuilder;
        }
        
        return compilation.CreateModuleBuilder(
            EmitOptions.Default,
            null,
            null,
            null,
            ImmutableArray<ResourceDescription>.Empty,
            null,
            DiagnosticBag.GetInstance(),
            CancellationToken.None
        ) as PEModuleBuilder;
    }

    public void SetEntryPoint(PEModuleBuilder moduleBuilder)
    {
        Debug.Assert(_compilationFinished);
        Debug.Assert(_functions.ContainsKey(EntryPointQualifiedName));
        var symbol = GetSymbol(moduleBuilder, GlobalClassName, EntryPointName) as MethodSymbol;

        var diagnostics = DiagnosticBag.GetInstance();
        moduleBuilder.SetPEEntryPoint(symbol, diagnostics);
        
        if (diagnostics.Count > 0)
        {
            foreach (var diagnostic in diagnostics.AsEnumerable())
            {
                Console.WriteLine(diagnostic.GetMessage());
            }

            throw new InvalidOperationException("Could not set entry point");
        }
    }
    
    public override void CompilationFinished()
    {
        Debug.Assert(_emitContext.Count == 0);
        base.CompilationFinished();
    }

    public override (Declaration, Symbol) VisitClassNode(ClassNode n)
    {
        //var classNode = SyntaxFactory.ClassDeclaration(n.Name);
        if (_classes.ContainsKey(n.Name))
        {
            var classSymbol =
                _globalNamespace.GetMembersUnordered().First(x => (x as NamedTypeSymbol).Name == n.Name) as SourceNamedTypeSymbol;
            _typeSymbols.Push(classSymbol);
            var members = new Dictionary<string, ImmutableArray<Symbol>>();
            foreach (var child in n.GetChildren().OfType<FuncDeclNode>())
            {
                var (_, symbol) = Visit(child);
                members[child.Name] = new List<Symbol> { symbol }.ToImmutableArray();
            }
            foreach (var child in n.GetChildren().OfType<VarDeclNode>())
            {
                var (_, symbol) = Visit(child);
                members[child.Variable.Name] = new List<Symbol> { symbol }.ToImmutableArray();
            }
            
            classSymbol.SetMembersDictionary(members);

            _typeSymbols.Pop();

            return (null, null);
        }
        
        _emitContext.Push(n.Name);
        _classes[n.Name] = ImmutableSegmentedDictionary<string, VoidResult>.Empty;
        foreach (var child in n.GetChildren())
        { 
            Visit(child);
        }
            
        var ctorName = n.Name + QualifiedNameSeparator + ".ctor";
        if (!_functions.ContainsKey(ctorName))
        {
            var ctorNode = new FuncDeclNode(
                new Token(TokenType.Id, ".ctor", 0, 0),
                new NoOpNode(),
                new List<Node>(), n.Name
            );

            ctorNode.Symbol = new FuncSymbol
            {
                Name = ".ctor",
                Body = new CompoundNode(new List<Node>()),
                ReturnType = new SemanticAnalysis.Symbols.TypeSymbol(n.Name),
                Parameters = new List<VarSymbol>(),
                Type = new SemanticAnalysis.Symbols.TypeSymbol("function")
            };
            
            _functions.Add(ctorName, ctorNode);
            _classes[n.Name] = _classes[n.Name].Add(".ctor", new VoidResult());
        }
        
        _emitContext.Pop();

        return (CreateClassDeclaration(n.Name, _classes[n.Name], n.Token.Line), null);

        /*var methodsAdded = n
            .GetChildren()
            .OfType<FuncDeclNode>();
        
        
        foreach (var method in methodsAdded)
        {
            _classes[n.Name] = _classes[n.Name].Add(method.Name, new VoidResult());
        }

        var fieldsAdded = n
            .GetChildren()
            .OfType<VarDeclNode>();
        
        foreach (var field in fieldsAdded)
        {
            _classes[n.Name] = _classes[n.Name].Add(field.Variable.Name, new VoidResult());
        }*/

        
    }

    public override (Declaration, Symbol) VisitFuncDeclNode(FuncDeclNode n)
    {
        if(_typeSymbols.Count == 0)
        {
            var className = _emitContext.Peek();
            var name = className == n.Name ? ".ctor" : n.Name;
            _classes[className] = _classes[className].Add(name, new VoidResult());
        }
        else
        {
            var symbol = _typeSymbols.Peek() as SourceNamedTypeSymbol;
            return (null, CreateMethodSymbol(symbol, n));
        }
        /*var parameters = n.Parameters.Select(
            x => SyntaxFactory
                .Parameter(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    SyntaxFactory.TokenList(),
                    SyntaxFactory.ParseTypeName(x.TypeSymbol.GetNetFullName()),
                    SyntaxFactory.ParseToken(x.Token.Value.ToString()),
                    null)
            );
        
        var methodNode = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName(n.ReturnType), name)
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
            .WithModifiers(GetKeywords(SyntaxKind.PublicKeyword));
            
        if (n.Name == "main")
        {
            methodNode = methodNode.WithModifiers(new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
        }*/
        
        Debug.Assert(_emitContext.Count == 1);
        _functions.Add(_emitContext.Peek() + QualifiedNameSeparator + n.Name, n);

        return (null, null);
    }

    public override (Declaration, Symbol) VisitVarDeclNode(VarDeclNode n)
    {
        if(_typeSymbols.Count == 0)
        {
            var className = _emitContext.Peek();
            _classes[className] = _classes[className].Add(n.Variable.Name, new VoidResult());
            return (null, null);
        }

        var symbol = _typeSymbols.Peek() as SourceNamedTypeSymbol;
        return (null, CreateFieldSymbol(symbol, n));

        /*var syntaxList = SyntaxFactory
            .SeparatedList<VariableDeclaratorSyntax>()
            .Add(SyntaxFactory.VariableDeclarator(n.Variable.Name));
        
        return SyntaxFactory
            .FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(n.TypeSymbol.Name), syntaxList))
            .WithModifiers(GetKeywords(SyntaxKind.PublicKeyword));*/
    }

    public override (Declaration, Symbol) VisitUseNode(UseNode n)
    {
        return (null, null);
    }

    private SyntaxTokenList GetKeywords(params SyntaxKind[] keywords)
    {
        return new SyntaxTokenList(keywords.Select(SyntaxFactory.Token));
    }
    
#if NET472
    private Assembly GetAssembly(string name)
    {
        return AppDomain
            .CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(asm => asm.GetName().Name == name);
    }
#endif

    private SingleTypeDeclaration CreateClassDeclaration(string name, ImmutableSegmentedDictionary<string, VoidResult> members, int position)
    {
        return new SingleTypeDeclaration(DeclarationKind.Class,
            name,
            0,
            DeclarationModifiers.Public,
            SingleTypeDeclaration.TypeDeclarationFlags.None,
            null,
            new LocationTest(0, position),
            members, 
            ImmutableArray<SingleTypeDeclaration>.Empty, 
            ImmutableArray<Diagnostic>.Empty, 
            QuickAttributes.None
        );
    }

    private SourceNamespaceSymbol CreateNamespaceSymbol(string name, ImmutableArray<SingleNamespaceOrTypeDeclaration> members)
    {
        var namespaceDeclaration = SingleNamespaceDeclaration.Create(name, true, false, null, null,
            members, ImmutableArray<Diagnostic>.Empty);
        var mergedNamespace = MergedNamespaceDeclaration.Create(namespaceDeclaration);
        return new SourceNamespaceSymbol(_compilation.SourceModule as SourceModuleSymbol, _compilation.SourceModule, mergedNamespace, BindingDiagnosticBag.GetInstance());
    }

    private SourceNamedTypeSymbol CreateClassSymbol(NamespaceOrTypeSymbol containingSymbol, SingleTypeDeclaration declaration)
    {
        var mergedArray = ImmutableArray<SingleTypeDeclaration>.Empty;
        mergedArray = mergedArray.Add(declaration);
        var merged = new MergedTypeDeclaration(mergedArray);
        return new SourceNamedTypeSymbol(containingSymbol, merged, BindingDiagnosticBag.GetInstance());
    }

    private MethodSymbolTest CreateMethodSymbol(SourceNamedTypeSymbol containingType, FuncDeclNode n)
    {
        var returnType = _compilation.Assembly.GetTypeByMetadataName(n.Symbol.ReturnType.GetNetFullName());
        var symbol = new MethodSymbolTest(containingType, false, returnType, n);
        int i = 0;
        var parameters = new List<ParameterSymbol>();
        foreach (var param in n.Parameters)
        {
            var type = _compilation.Assembly.GetTypeByMetadataName(param.TypeSymbol.GetNetFullName());
            var paramSymbol = new ParameterSymbolTest(symbol, i, type);
            parameters.Add(paramSymbol);
            ++i;
        }
        
        symbol.SetParameters(parameters.ToImmutableArray());

        return symbol;
    }

    private FieldSymbolTest CreateFieldSymbol(SourceNamedTypeSymbol containingType, VarDeclNode n)
    {
        var type = _compilation.Assembly.GetTypeByMetadataName(n.TypeSymbol.GetNetFullName());
        return new FieldSymbolTest(containingType, n.Variable.Name, type);
    }
}

class MethodSymbolTest : SourceMemberMethodSymbol
{
    public MethodSymbolTest(NamedTypeSymbol containingType, bool isIterator, TypeSymbol returnType, FuncDeclNode n) : base(containingType, null, null, isIterator)
    {
        _parameterSymbols = ImmutableArray<ParameterSymbol>.Empty;
        _returnType = TypeWithAnnotations.Create(returnType);
        _sortKey = new LexicalSortKey(0, n.Token.Line);
    }

    internal override LexicalSortKey GetLexicalSortKey()
    {
        return _sortKey;
    }

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

    internal override bool IsExpressionBodied { get => false; }

    private LexicalSortKey _sortKey;
}

class ParameterSymbolTest : ParameterSymbol
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

    public ParameterSymbolTest(Symbol containingSymbol, int ordinal, TypeSymbol type)
    {
        ContainingSymbol = containingSymbol;
        Ordinal = ordinal;
        TypeWithAnnotations = TypeWithAnnotations.Create(type);
    }
}

class FieldSymbolTest : SourceFieldSymbol
{
    public FieldSymbolTest(SourceMemberContainerTypeSymbol containingType, string name, TypeSymbol type) : base(containingType)
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
