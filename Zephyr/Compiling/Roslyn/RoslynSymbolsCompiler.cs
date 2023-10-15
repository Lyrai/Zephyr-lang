using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;
using Zephyr.SyntaxAnalysis.ASTNodes;
using BindingDiagnosticBag = Microsoft.CodeAnalysis.CSharp.BindingDiagnosticBag;

namespace Zephyr.Compiling.Roslyn;

internal class RoslynSymbolsCompiler : BaseRoslynCompiler<Symbol>
{
    private Stack<TypeSymbol> _typeSymbols = new();
    private ImmutableArray<SingleNamespaceOrTypeDeclaration> _globalNamespaceMembers;
    private SourceNamespaceSymbol _globalNamespace;

    public RoslynSymbolsCompiler(RoslynDeclarationsCompiler compiler,
        ImmutableArray<SingleNamespaceOrTypeDeclaration> globalNamespaceMembers)
    {
        _globalNamespaceMembers = globalNamespaceMembers;
        _compilation = compiler.Compilation;
    }

    public void CreateSymbols(Node n)
    {
        var node = n as CompoundNode;
        _globalNamespace = CreateNamespaceSymbol("<global namespace>", _globalNamespaceMembers);
        var globalClassMembers = new Dictionary<string, ImmutableArray<Symbol>>();
        var globalClassSymbol = _globalNamespace.GetMembers(GlobalClassName)[0] as SourceNamedTypeSymbol;
        
        foreach (var child in node.GetChildren().Where(node => node is not UseNode))
        {
            if (child is ClassNode)
            {
                Visit(child);
                continue;
            }

            _typeSymbols.Push(globalClassSymbol);
            var symbol = Visit(child);
            if (symbol is ZephyrMethodSymbol s)
            {
                s.SetStatic();
            }

            globalClassMembers[(child as FuncDeclNode).Name] = new List<Symbol> { symbol }.ToImmutableArray();
            _typeSymbols.Pop();
        }

        globalClassSymbol.SetMembersDictionary(globalClassMembers);

        foreach (var symbol in _globalNamespace.GetMembersUnordered().OfType<SourceNamedTypeSymbol>())
        {
            symbol.ResetMembersAndInitializers();
            symbol.GetMembersAndInitializers();
        }

        (_compilation.SourceModule as SourceModuleSymbol).SetGlobalNamespace(_globalNamespace);
    }

    public override Symbol VisitClassNode(ClassNode n)
    {
        var classSymbol = _globalNamespace
                .GetMembers(n.Name)[0]
            as SourceNamedTypeSymbol;
        _typeSymbols.Push(classSymbol);

        var members = new Dictionary<string, ImmutableArray<Symbol>>();
        foreach (var child in n.GetChildren().OfType<FuncDeclNode>())
        {
            var symbol = Visit(child);
            members[child.Name] = new List<Symbol> { symbol }.ToImmutableArray();
        }

        foreach (var child in n.GetChildren().OfType<VarDeclNode>())
        {
            var symbol = Visit(child);
            members[child.Variable.Name] = new List<Symbol> { symbol }.ToImmutableArray();
        }

        classSymbol.SetMembersDictionary(members);

        _typeSymbols.Pop();

        return null;
    }

    public override Symbol VisitFuncDeclNode(FuncDeclNode n)
    {
        var symbol = _typeSymbols.Peek() as SourceNamedTypeSymbol;
        return CreateMethodSymbol(symbol, n);
    }

    public override Symbol VisitVarDeclNode(VarDeclNode n)
    {
        var symbol = _typeSymbols.Peek() as SourceNamedTypeSymbol;
        return CreateFieldSymbol(symbol, n);
    }

    public override Symbol VisitUseNode(UseNode n)
    {
        return null;
    }

    private SourceNamespaceSymbol CreateNamespaceSymbol(string name,
        ImmutableArray<SingleNamespaceOrTypeDeclaration> members)
    {
        var namespaceDeclaration = SingleNamespaceDeclaration.Create(name, true, false, null, null,
            members, ImmutableArray<Diagnostic>.Empty);
        var mergedNamespace = MergedNamespaceDeclaration.Create(namespaceDeclaration);
        return new SourceNamespaceSymbol(_compilation.SourceModule as SourceModuleSymbol, _compilation.SourceModule,
            mergedNamespace, BindingDiagnosticBag.GetInstance());
    }

    private ZephyrMethodSymbol CreateMethodSymbol(SourceNamedTypeSymbol containingType, FuncDeclNode n)
    {
        var returnType = _compilation.Assembly.GetType(n.Symbol.ReturnType);
        var symbol = new ZephyrMethodSymbol(containingType, false, returnType, n);
        int i = 0;
        var parameters = new List<ParameterSymbol>();
        foreach (var param in n.Parameters)
        {
            var type = _compilation.Assembly.GetType(param.TypeSymbol);
            var paramSymbol = new ZephyrParameterSymbol(symbol, i, type);
            parameters.Add(paramSymbol);
            ++i;
        }

        symbol.SetParameters(parameters.ToImmutableArray());

        return symbol;
    }

    private ZephyrFieldSymbol CreateFieldSymbol(SourceNamedTypeSymbol containingType, VarDeclNode n)
    {
        var type = _compilation.Assembly.GetType(n.TypeSymbol);
        return new ZephyrFieldSymbol(containingType, n.Variable.Name, type);
    }
}
