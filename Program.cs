using System;
using System.Diagnostics;
using System.IO;
using Zephyr.Interpreting;
using Zephyr.LexicalAnalysis;
using Zephyr.SemanticAnalysis;
using Zephyr.SyntaxAnalysis;
using Antlr4.Runtime;
using Lexer = Zephyr.LexicalAnalysis.Lexer;
using Parser = Zephyr.SyntaxAnalysis.Parser;

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
                var c = File.ReadAllLines("../../../test.txt");
                string code = string.Join('\n', c);
                ICharStream stream = CharStreams.fromString(code);
                ITokenSource l = new testLexer(stream);
                ITokenStream tokenStream = new CommonTokenStream(l);
                int i = 1;
                while (tokenStream.LT(i).Type != -1)
                {
                    var token = tokenStream.LT(i);
                    Console.WriteLine($"Token {token.Type} text {token.Text}");
                    ++i;
                }
                /*Lexer lexer = new Lexer(code);
                lexer.Analyze();
                var tokens = lexer.GetTokens();

                Parser parser = new Parser(tokens);
                var tree = parser.Parse();

                SemanticAnalyzer analyzer = new SemanticAnalyzer(tree);
                analyzer.Analyze();
                sw.Stop();
                Console.WriteLine($"Analysis in {sw.ElapsedMilliseconds}ms");
                
                sw.Reset();
                sw.Start();
                Interpreter interpreter = new Interpreter(tree);
                if(_hasError == false)
                    interpreter.Interpret();
                sw.Stop();
                Console.WriteLine($"Executed in {sw.ElapsedMilliseconds}ms");*/
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