using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using Zephyr.Interpreting;
using Zephyr.SemanticAnalysis;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Zephyr.Compiling;
using Zephyr.Compiling.Roslyn;
using BindingDiagnosticBag = Microsoft.CodeAnalysis.CSharp.BindingDiagnosticBag;
using MethodBody = Microsoft.CodeAnalysis.CodeGen.MethodBody;
using MethodCompiler = Microsoft.CodeAnalysis.CSharp.MethodCompiler;

namespace Zephyr
{
    class Zephyr
    {
        private static bool _hasError;

        static void Main(string[] args)
        {
            /*var syntax = SyntaxFactory
                .MethodDeclaration(SyntaxFactory.ParseTypeName("int"), "Main")
                .WithModifiers(new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
            var typedecl1 = SyntaxFactory.ClassDeclaration("TestClass").AddMembers(syntax);
            var tree1 = CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit().AddMembers(typedecl1));
            var g = CSharpCompilation.Create(
                "test",
                new[] { tree1 },
                new[] {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Console").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
                    MetadataReference.CreateFromFile(typeof(Stopwatch).Assembly.Location),
                },
                new CSharpCompilationOptions(
                    outputKind: OutputKind.ConsoleApplication,
                    optimizationLevel: OptimizationLevel.Release
                )
            );
            var mb = g.CreateModuleBuilder(EmitOptions.Default, null, null,
                null, ImmutableArray<ResourceDescription>.Empty, null, DiagnosticBag.GetInstance(),
                CancellationToken.None) as PEModuleBuilder;
            var bindingdiag = new BindingDiagnosticBag(DiagnosticBag.GetInstance());
            var compiler = new MethodCompiler(
                g,
                mb,
                false,
                false,
                true,
                bindingdiag,
                null,
                null,
                CancellationToken.None);
            compiler.CompileSynthesizedMethods(mb.GetEmbeddedTypes(bindingdiag), bindingdiag);
            var ns = mb.SourceModule.GlobalNamespace;
            var tt = ns.GetMembers()[0] as SourceNamedTypeSymbol;
            var method = tt.GetMembers("Main")[0] as SourceOrdinaryMethodSymbol;
            var ilb = new ILBuilder(mb, new LocalSlotManager(null), OptimizationLevel.Release, true);
            ilb.EmitIntConstant(3);
            ilb.EmitIntConstant(2);
            ilb.EmitOpCode(ILOpCode.Add);
            ilb.EmitRet(false);
            ilb.Realize();
            var il = ilb.RealizedIL;
            var methodbody = new MethodBody(
                il,
                ilb.MaxStack,
                method.GetCciAdapter(),
                new DebugId(0, mb.CurrentGenerationOrdinal),
                ilb.LocalSlotManager.LocalsInOrder(),
                ilb.RealizedSequencePoints,
                null,
                ilb.RealizedExceptionHandlers,
                ilb.AreLocalsZeroed,
                false,
                ilb.GetAllScopes(),
                ilb.HasDynamicLocal,
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
                false
                );
            ilb.FreeBasicBlocks();
            mb.SetMethodBody(method, methodbody);
            mb.SetPEEntryPoint(method, DiagnosticBag.GetInstance());
            var ctor = tt.GetMembers().Where(m => m is SynthesizedInstanceConstructor).AsImmutable()[0] as SynthesizedInstanceConstructor;
            var ilb1 = new ILBuilder(mb, new LocalSlotManager(null), OptimizationLevel.Release, true);
            ilb1.Realize();
            var ctorbody = new MethodBody(
                ilb1.RealizedIL,
                ilb1.MaxStack,
                ctor.GetCciAdapter(),
                new DebugId(0, mb.CurrentGenerationOrdinal),
                ilb1.LocalSlotManager.LocalsInOrder(),
                ilb1.RealizedSequencePoints,
                null,
                ilb1.RealizedExceptionHandlers,
                ilb1.AreLocalsZeroed,
                false,
                ilb1.GetAllScopes(),
                ilb1.HasDynamicLocal,
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
                true
                );
            mb.SetMethodBody(ctor, ctorbody);
            mb.CompilationFinished();
            var diag = DiagnosticBag.GetInstance();
            var peStreamProvider =
                new Compilation.SimpleEmitStreamProvider(File.Open(@"E:\Projects\test-zephyr-emit\test.exe",
                    FileMode.Create));
            /*var success = g.SerializeToPeStream(
                mb,
                peStreamProvider,
                null,
                null,
                null,
                null,
                diag,
                EmitOptions.Default,
                null,
                CancellationToken.None
            );
            if (diag.HasAnyErrors())
            {
                foreach (var d in diag.AsEnumerable())
                {
                    Console.WriteLine(d.GetMessage());
                }
            }

            if (!success)
            {
                Console.WriteLine("Failed");
            }*/
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                var c = File.ReadAllLines("../../test_IL.txt");
                string code = string.Join('\n', c);
                ICharStream stream = CharStreams.fromString(code);
                ITokenSource l = new ZephyrLexer(stream);
                ITokenStream tokenStream = new CommonTokenStream(l);
                var parser = new ZephyrParser(tokenStream);
                var tree = parser.program();
                var visitor = new TestVisitor();
                var nodeTree = visitor.Visit(tree);

                SemanticAnalyzer analyzer = new SemanticAnalyzer(nodeTree);
                analyzer.Analyze();
                sw.Stop();
                Console.WriteLine($"Analysis in {sw.ElapsedMilliseconds}ms");

                sw.Reset();

                if (_hasError)
                    return;

                sw.Start();
                //Interpreter interpreter = new Interpreter(nodeTree);
                //interpreter.Interpret();
                //sw.Stop();
                //var compiler1 = new ExpressionsCompiler(nodeTree);
                //var entry = compiler1.Compile();
                //sw.Start();
                //entry.Invoke(null, null);
                var roslynCompiler = new RoslynDeclarationsCompiler("Test");
                var functions = roslynCompiler.Compile(nodeTree);
                var expressionCompiler = new RoslynExpressionCompiler(roslynCompiler);
                expressionCompiler.Compile(functions);
                expressionCompiler.CompilationFinished();
                var diagnostics = DiagnosticBag.GetInstance();
                var succ = expressionCompiler.Emit("test.dll", diagnostics);
                if (!succ)
                {
                    Console.WriteLine("Failed");
                }
                if (diagnostics.HasAnyErrors())
                {
                    foreach (var diagnostic in diagnostics.AsEnumerable())
                    {
                        Console.WriteLine(diagnostic.GetMessage());
                    }
                }
                sw.Stop();
                Console.WriteLine($"Executed in {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public static void Error(Exception e)
        {
            _hasError = true;
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}
