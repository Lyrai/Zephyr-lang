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
    class Program
    {
        private static bool _hasError;

        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                var options = ParseArgs(args);
                sw.Start();
                var c = File.ReadAllLines(options["input"]);
                string code = string.Join("\n", c);
                ICharStream stream = CharStreams.fromString(code);
                ITokenSource l = new ZephyrLexer(stream);
                ITokenStream tokenStream = new CommonTokenStream(l);
                var parser = new ZephyrParser(tokenStream);
                var tree = parser.program();
                var visitor = new ParseTreeVisitor();
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
                var roslynCompiler = new RoslynDeclarationsCompiler("Test");
                var functions = roslynCompiler.Compile(nodeTree);
                var expressionCompiler = new RoslynExpressionCompiler(roslynCompiler);
                expressionCompiler.Compile(functions);
                expressionCompiler.CompilationFinished();
                var diagnostics = DiagnosticBag.GetInstance();
                var succ = expressionCompiler.Emit(options["output"], diagnostics);
                if (!succ)
                {
                    Console.WriteLine("Compilation failed");
                }
                if (diagnostics.HasAnyErrors())
                {
                    foreach (var diagnostic in diagnostics.AsEnumerable())
                    {
                        Console.WriteLine(diagnostic.GetMessage());
                    }
                }
                sw.Stop();
                Console.WriteLine($"Compiled in {sw.ElapsedMilliseconds}ms");
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

        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            var options = new Dictionary<string, string>();

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-o":
                    case "--output":
                        i++;
                        options["output"] = args[i];
                        break;
                    
                    default:
                        options["input"] = args[i];
                        break;
                }
            }

            return options;
        }
    }
}
