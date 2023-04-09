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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling.Roslyn;

internal class RoslynDeclarationsCompiler: BaseRoslynCompiler<MemberDeclarationSyntax>
{
    private readonly Dictionary<string, Node> _functions = new();

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

    private readonly Stack<string> _emitContext = new();

    public ImmutableDictionary<string, Node> Compile(Node n)
    {
        RestartCompiler();
        
        Debug.Assert(n is CompoundNode);
        var node = n as CompoundNode;
        ClassDeclarationSyntax globalClass = null;
        var compilationUnit = SyntaxFactory.CompilationUnit();
        
        foreach (var child in node.GetChildren())
        {
            if (child is ClassNode)
            {
                compilationUnit = compilationUnit.AddMembers(Visit(child));
                continue;
            }

            globalClass ??= SyntaxFactory
                .ClassDeclaration(GlobalClassName)
                .WithModifiers(GetKeywords(SyntaxKind.StaticKeyword));

            _emitContext.Push(GlobalClassName);
            var member = Visit(child)
                .WithModifiers(GetKeywords(SyntaxKind.StaticKeyword));
            globalClass = globalClass.AddMembers(member);
            _emitContext.Pop();
        }

        if (globalClass is not null)
        {
            compilationUnit = compilationUnit.AddMembers(globalClass);
        }

        _compilation = _compilation.AddSyntaxTrees(CSharpSyntaxTree.Create(compilationUnit));
        
        return _functions.ToImmutableDictionary();
    }

    protected override void RestartCompiler()
    {
        _functions.Clear();
        _emitContext.Clear();
        base.RestartCompiler();
    }

    public PEModuleBuilder CreateModuleBuilder(CSharpCompilation compilation)
    {
        Debug.Assert(_compilationFinished);
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

    public override MemberDeclarationSyntax VisitClassNode(ClassNode n)
    {
        var classNode = SyntaxFactory.ClassDeclaration(n.Name);
        
        _emitContext.Push(n.Name);
        var methodsAdded = n
            .GetChildren()
            .OfType<FuncDeclNode>()
            .Aggregate(classNode,
                (current, method) => current.AddMembers(Visit(method))
            );

        var fieldsAdded = n
            .GetChildren()
            .OfType<VarDeclNode>()
            .Aggregate(methodsAdded,
                (current, field) => current.AddMembers(Visit(field))
            );

        var ctorName = n.Name + QualifiedNameSeparator + ".ctor";
        if (!_functions.ContainsKey(ctorName))
        {
            var ctorNode = new FuncDeclNode(
                new Token(TokenType.Id, ".ctor", 0, 0),
                new List<Node> { new NoOpNode() },
                new List<Node>(), n.Name
            );
            
            _functions.Add(ctorName, ctorNode);
        }
        
        _emitContext.Pop();

        return fieldsAdded;
    }

    public override MemberDeclarationSyntax VisitFuncDeclNode(FuncDeclNode n)
    {
        var name = _emitContext.Peek() == n.Name ? ".ctor" : n.Name;
        var parameters = n.Parameters.Select(
            x => SyntaxFactory
                .Parameter(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    SyntaxFactory.TokenList(),
                    SyntaxFactory.ParseTypeName(x.TypeSymbol.Name),
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
        }
        
        Debug.Assert(_emitContext.Count == 1);
        _functions.Add(_emitContext.Peek() + QualifiedNameSeparator + n.Name, n);

        return methodNode;
    }

    public override MemberDeclarationSyntax VisitVarDeclNode(VarDeclNode n)
    {
        var syntaxList = SyntaxFactory
            .SeparatedList<VariableDeclaratorSyntax>()
            .Add(SyntaxFactory.VariableDeclarator(n.Variable.Name));
        
        return SyntaxFactory
            .FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(n.TypeSymbol.Name), syntaxList))
            .WithModifiers(GetKeywords(SyntaxKind.PublicKeyword));
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
}
