using System.Diagnostics;
using Zephyr.SemanticAnalysis;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis;
using Zephyr.Compiling.Roslyn;

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
#if DEBUG
                Console.WriteLine(e.StackTrace);
#endif
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

            if (!options.ContainsKey("input"))
            {
                Console.WriteLine("No input specified");
                Environment.Exit(1);
            }

            if (!File.Exists(options["input"]))
            {
                Console.WriteLine($"Cannot access {options["input"]}");
                Environment.Exit(1);
            }

            if (!options.ContainsKey("output"))
            {
                options["output"] = Path.ChangeExtension(Path.GetFileName(options["input"]), "exe");
            }

            return options;
        }
    }
}
