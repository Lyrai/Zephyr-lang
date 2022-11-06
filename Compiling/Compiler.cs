using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Zephyr.Compiling.Contexts;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr.Compiling
{
    public class Compiler: INodeVisitor<object>
    {
        private AssemblyBuilder _assemblyBuilder;
        private CompilationContext _context;
        private Node _tree;
        private MethodInfo _entryPoint = null!;

        public Compiler(Node tree)
        {
            _assemblyBuilder =
                AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Main"), AssemblyBuilderAccess.Run);
            _context = null!;
            _tree = tree;
        }

        public static void Test()
        {
            AssemblyName name = new AssemblyName("TestAssembly");
            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = builder.DefineDynamicModule(name.Name);
            TypeBuilder typeBuilder = moduleBuilder.DefineType("TestType", TypeAttributes.Public);
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                "Ttt",
                MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.Private,
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
            Type type = typeBuilder.CreateType();
            type.InvokeMember("Ttt", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, null, null);
        }

        public MethodInfo Compile()
        {
            _context = new ModuleContext("Main", null, _assemblyBuilder.DefineDynamicModule("Main"));
            Visit(_tree);
            return _entryPoint;
        }
        
        public object VisitClassNode(ClassNode n)
        {
            if (_context is not ModuleContext ctx)
            {
                throw new InvalidCastException("Wrong context type: expected module");
            }

            var mb = ctx.Builder;
            var classBuilder = mb.DefineType(n.Name, TypeAttributes.Class, ctx.GetType(n.Parent.Name));

            _context = new ClassContext(n.Name, _context, classBuilder);
            foreach (var node in n.Body)
            {
                Visit(node);
            }

            var type = classBuilder.CreateType();
            ctx.AddType(n.Name, type);

            return type;
        }

        public object VisitGetNode(GetNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitCompoundNode(CompoundNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitBinOpNode(BinOpNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitUnOpNode(UnOpNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitLiteralNode(LiteralNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitIfNode(IfNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitWhileNode(WhileNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitVarNode(VarNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitVarDeclNode(VarDeclNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitPropertyDeclNode(PropertyDeclNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitFuncCallNode(FuncCallNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitFuncDeclNode(FuncDeclNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitReturnNode(ReturnNode n)
        {
            throw new System.NotImplementedException();
        }

        public object VisitNoOpNode(NoOpNode n)
        {
            throw new System.NotImplementedException();
        }

        private object Visit(Node n)
        {
            return n.Accept(this);
        }
    }
}