using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SemanticAnalysis.Symbols;
using Zephyr.SyntaxAnalysis.ASTNodes;
using BindingDiagnosticBag = Microsoft.CodeAnalysis.CSharp.BindingDiagnosticBag;
using ParameterSymbol = Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol;
using Symbol = Microsoft.CodeAnalysis.CSharp.Symbol;

namespace Zephyr.Compiling.Roslyn;

internal class RoslynDeclarationsCompiler : BaseRoslynCompiler<Declaration>
{
    private readonly Dictionary<string, Node> _functions = new();
    private PEModuleBuilder _moduleBuilder;
    private readonly Stack<string> _emitContext = new();
    private Dictionary<string, ImmutableSegmentedDictionary<string, VoidResult>> _classes = new();

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

    public (ImmutableDictionary<string, Node>, ImmutableArray<SingleNamespaceOrTypeDeclaration>) Compile(Node n)
    {
        RestartCompiler();

        Debug.Assert(n is CompoundNode);
        var node = n as CompoundNode;

        var globalNamespaceMembers = ImmutableArray<SingleNamespaceOrTypeDeclaration>.Empty;
        _classes[GlobalClassName] = ImmutableSegmentedDictionary<string, VoidResult>.Empty;

        foreach (var child in node.GetChildren().Where(node => node is not UseNode))
        {
            if (child is ClassNode)
            {
                var decl = Visit(child);
                globalNamespaceMembers = globalNamespaceMembers.Add((SingleTypeDeclaration)decl);
                continue;
            }

            _emitContext.Push(GlobalClassName);
            Visit(child);
            _emitContext.Pop();
        }

        if (!_functions.ContainsKey(GlobalClassName + QualifiedNameSeparator + ".ctor"))
        {
            var ctorName = GlobalClassName + QualifiedNameSeparator + ".ctor";
            if (!_functions.ContainsKey(ctorName))
            {
                var ctorNode = new FuncDeclNode(
                    new Token(TokenType.Id, ".ctor", 0, 0),
                    new NoOpNode(),
                    new List<Node>(), GlobalClassName
                );

                ctorNode.Symbol = new FuncSymbol
                {
                    Name = ".ctor",
                    Body = new CompoundNode(new List<Node>()),
                    ReturnType = new SemanticAnalysis.Symbols.TypeSymbol(GlobalClassName),
                    Parameters = new List<VarSymbol>(),
                    Type = new SemanticAnalysis.Symbols.TypeSymbol("function")
                };

                _functions.Add(ctorName, ctorNode);
                _classes[GlobalClassName] = _classes[GlobalClassName].Add(".ctor", new VoidResult());
            }
        }

        var globalClass = CreateClassDeclaration(GlobalClassName, _classes[GlobalClassName], 0);
        globalNamespaceMembers = globalNamespaceMembers.Add(globalClass);

        return (_functions.ToImmutableDictionary(), globalNamespaceMembers);
    }

    protected override void RestartCompiler()
    {
        _functions.Clear();
        _emitContext.Clear();
        base.RestartCompiler();
    }

    public PEModuleBuilder GetModuleBuilder(CSharpCompilation compilation)
    {
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
        var symbol = GetSymbol(moduleBuilder, GlobalClassName, EntryPointName) as ZephyrMethodSymbol;

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

    public override Declaration VisitClassNode(ClassNode n)
    {
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

        return CreateClassDeclaration(n.Name, _classes[n.Name], n.Token.Line);
    }

    public override Declaration VisitFuncDeclNode(FuncDeclNode n)
    {
        var className = _emitContext.Peek();
        var name = className == n.Name ? ".ctor" : n.Name;
        _classes[className] = _classes[className].Add(name, new VoidResult());

        Debug.Assert(_emitContext.Count == 1);
        _functions.Add(_emitContext.Peek() + QualifiedNameSeparator + n.Name, n);

        return null;
    }

    public override Declaration VisitVarDeclNode(VarDeclNode n)
    {
        var className = _emitContext.Peek();
        _classes[className] = _classes[className].Add(n.Variable.Name, new VoidResult());
        return null;
    }

    public override Declaration VisitUseNode(UseNode n)
    {
        return null;
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

    private SingleTypeDeclaration CreateClassDeclaration(string name,
        ImmutableSegmentedDictionary<string, VoidResult> members, int position)
    {
        return new SingleTypeDeclaration(DeclarationKind.Class,
            name,
            0,
            DeclarationModifiers.Public,
            SingleTypeDeclaration.TypeDeclarationFlags.HasAnyNontypeMembers,
            null,
            new ZephyrSourceLocation(0, position),
            members,
            ImmutableArray<SingleTypeDeclaration>.Empty,
            ImmutableArray<Diagnostic>.Empty,
            QuickAttributes.None
        );
    }
}
