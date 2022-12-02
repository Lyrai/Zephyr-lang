using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Zephyr.Compiling.Contexts;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling
{
    public class ExpressionsCompiler: BaseCompiler
    {
        private readonly Node _tree;
        private MethodInfo _entryPoint = null!;

        public ExpressionsCompiler(Node tree)
        {
            _context = new AssemblyContext("Main", null);
            _tree = tree;
        }

        public static void Test()
        {
            AssemblyName name = new AssemblyName("TestAssembly");
            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = builder.DefineDynamicModule(name.Name);
            TypeBuilder typeBuilder = moduleBuilder.DefineType("TestType", TypeAttributes.Public);
            MethodBuilder methodBuilder = moduleBuilder.DefineGlobalMethod(
                "Ttt",
                MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.Public,
                typeof(void),
                null
            );
            ILGenerator generator = methodBuilder.GetILGenerator();
            generator.DeclareLocal(typeof(int));
            generator.Emit(OpCodes.Ldc_I4, 5);
            generator.Emit(OpCodes.Ldc_I4, 6);
            generator.Emit(OpCodes.Add);
            generator.Emit(OpCodes.Stloc_0);
            Type[] wlParams = {typeof(string),
                typeof(object)};
            MethodInfo wrln = typeof(Console).GetMethod("WriteLine", wlParams);
            string str = "Sum is {0}";
            generator.Emit(OpCodes.Ldstr, str);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Box, typeof(int));
            generator.EmitCall(OpCodes.Call, wrln, null);
            //generator.Emit(OpCodes.Pop);
            //generator.Emit(OpCodes.Pop);
            generator.Emit(OpCodes.Ret);
            moduleBuilder.CreateGlobalFunctions();
            var g = moduleBuilder.GetMethod("Ttt");
            Type type = typeBuilder.CreateType();
            type.InvokeMember("Ttt", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, null, null);
        }

        public MethodInfo Compile()
        {
            _context = _context.DefineModule("Main") ?? throw new InvalidOperationException("Could not define module \"Main\"");
            Visit(_tree);
            //_context.CompleteFunction();
            return _entryPoint;
        }
        
        public object VisitClassNode(ClassNode n)
        {
            var oldContext = _context;
            _context = _context.DefineType(n.Name, _context.GetTypeByName(n.Parent.Name)) 
                       ?? throw new InvalidOperationException($"Could not define class {n.Name}");
            
            foreach (var node in n.Body)
            {
                Visit(node);
            }

            _context.CreateType();
            _context = oldContext;

            return null!;
        }

        public object VisitGetNode(GetNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitCompoundNode(CompoundNode n)
        {
            var last = n.GetChildren().Last();
            foreach (var node in n.GetChildren())
            {
                Visit(node);
                if(node is FuncCallNode && node != last)
                    _context.GetILGenerator().Emit(OpCodes.Pop);
            }

            return null!;
        }

        public object VisitBinOpNode(BinOpNode n)
        {
            var generator = _context.GetILGenerator();
            if (n.Token.Type == TokenType.Assign)
            {
                if (n.Left is VarDeclNode)
                    Visit(n.Left);
                
                Visit(n.Right);
                generator.Emit(OpCodes.Stloc, _context.GetVariable((string) n.Left.Token.Value));
                return null!;
            }
            
            Visit(n.Left);
            Visit(n.Right);

            var op = n.Token.Type switch
            {
                TokenType.Plus => OpCodes.Add,
                TokenType.Minus => OpCodes.Sub,
                TokenType.Multiply => OpCodes.Mul,
                TokenType.Divide => OpCodes.Div,
                TokenType.Equal => OpCodes.Ceq,
                TokenType.NotEqual => OpCodes.Ceq,
                TokenType.Greater => OpCodes.Cgt,
                TokenType.Less => OpCodes.Clt
            };

            generator.Emit(op);
            
            if (n.Token.Type == TokenType.NotEqual)
            {
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ceq);
            }
            
            return null!;
        }

        public object VisitUnOpNode(UnOpNode n)
        {
            var generator = _context.GetILGenerator()!;
            if (n.Token.Type == TokenType.Print)
            {
                Type[] wlParams = {typeof(object)};
                MethodInfo wrln = typeof(Console).GetMethod("WriteLine", wlParams);
                Visit(n.Operand);
                generator.Emit(OpCodes.Box, MapType(n.Operand.TypeSymbol));
                generator.EmitCall(OpCodes.Call, wrln, null);
            }
            else if (n.Token.Type == TokenType.Minus)
            {
                Visit(n.Operand);
                generator.Emit(OpCodes.Neg);
            }
            else
            {
                throw new InvalidOperationException($"Invalid unary operation: {n.Token.Value}");
            }

            return null!;
        }

        public object VisitLiteralNode(LiteralNode n)
        {
            var generator = _context.GetILGenerator();
            if (generator is null)
            {
                throw new InvalidOperationException($"Statement in illegal context: {_context}");
            }
            
            switch (n.Token.Type)
            {
                case TokenType.Integer:
                    generator.Emit(OpCodes.Ldc_I4, (int) n.Value);
                    break;
                case TokenType.DoubleLit:
                    generator.Emit(OpCodes.Ldc_R8, (double) n.Value);
                    break;
                case TokenType.True:
                    generator.Emit(OpCodes.Ldc_I4, 1);
                    break;
                case TokenType.False:
                    generator.Emit(OpCodes.Ldc_I4, 0);
                    break;
                case TokenType.StringLit:
                    generator.Emit(OpCodes.Ldstr, (string) n.Value);
                    break;
            }
            
            return null!;
        }

        public object VisitIfNode(IfNode n)
        {
            Visit(n.Condition);
            var generator = _context.GetILGenerator();
            var elseLabel = generator.DefineLabel();
            var doneLabel = generator.DefineLabel();
            generator.Emit(OpCodes.Brfalse, elseLabel);
            
            Visit(n.ThenBlock);
            
            generator.Emit(OpCodes.Br, doneLabel);
            generator.MarkLabel(elseLabel);
            
            if (n.ElseBlock is not null)
                Visit(n.ElseBlock);

            generator.MarkLabel(doneLabel);
            return null!;
        }

        public object VisitWhileNode(WhileNode n)
        {
            var generator = _context.GetILGenerator();
            var begin = generator.DefineLabel();
            generator.MarkLabel(begin);
            Visit(n.Condition);
            var done = generator.DefineLabel();
            generator.Emit(OpCodes.Brfalse, done);
            Visit(n.Body);
            generator.Emit(OpCodes.Br, begin);
            generator.MarkLabel(done);

            return null!;
        }

        public object VisitVarNode(VarNode n)
        {
            _context.GetILGenerator().Emit(OpCodes.Ldloc, _context.GetVariable(n.Name));
            
            return null!;
        }

        public object VisitVarDeclNode(VarDeclNode n)
        {
            _context.DefineVariable(n.Variable.Name, MapType(n.TypeSymbol));

            return null!;
        }

        public object VisitPropertyDeclNode(PropertyDeclNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitFuncCallNode(FuncCallNode n)
        {
            var generator = _context.GetILGenerator();
            var args = n.Arguments.Select(node => node.TypeSymbol).Select(MapType).ToList();
            var function = _context.GetFunction(n.Name, args);
            foreach (var node in n.Arguments)
                Visit(node);
            
            generator.EmitCall(OpCodes.Call, function, null);
            return null!;
        }

        public object VisitFuncDeclNode(FuncDeclNode n)
        {
            var types = new List<Type>();
            foreach (var symbol in n.Symbol.Parameters)
                types.Add(MapType(symbol.Type));
            
            
            var oldContext = _context;
            _context = _context.DefineFunction(n.Name, types, MapType(n.Symbol.ReturnType)) ?? throw new InvalidOperationException($"Cannot define function in {_context}");
            foreach (var node in n.Body)
                Visit(node);

            if (n.ReturnType == "void" && n.Body.Last() is not ReturnNode)
                _context.GetILGenerator().Emit(OpCodes.Ret);

            _context = oldContext;
            if (_context is ModuleContext)
                _context.CompleteFunctions();
            if (_context is ModuleContext && n.Name == "main")
                _entryPoint = _context.LastFunction()!;
            
            return null!;
        }

        public object VisitReturnNode(ReturnNode n)
        {
            _context.GetILGenerator()?.Emit(OpCodes.Ret);
            return null!;
        }

        public object VisitNoOpNode(NoOpNode n)
        {
            return null!;
        }

        private object Visit(Node n)
        {
            return n.Accept(this);
        }
    }
}