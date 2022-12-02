using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Zephyr.Interpreting;
using Zephyr.SemanticAnalysis;
using Antlr4.Runtime;
using Zephyr.Compiling;

namespace Zephyr
{
    class Zephyr
    {
        private static bool _hasError;

        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                var c = File.ReadAllLines("../../../test_IL.txt");
                string code = string.Join('\n', c);
                ICharStream stream = CharStreams.fromString(code);
                ITokenSource l = new ZephyrLexer(stream);
                ITokenStream tokenStream = new CommonTokenStream(l);
                var parser = new ZephyrParser(tokenStream);
                var tree = parser.program();
                var visitor = new TestVisitor();
                var nodeTree = visitor.Visit(tree);

                /*Lexer lexer = new Lexer(code);
                lexer.Analyze();
                var tokens = lexer.GetTokens();

                Parser parser = new Parser(tokens);
                var nodeTree = parser.Parse();*/

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
                var compiler = new ExpressionsCompiler(nodeTree);
                var entry = compiler.Compile();
                //sw.Start();
                entry.Invoke(null, null); 
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