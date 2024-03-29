﻿using System.Globalization;
using Antlr4.Runtime.Tree;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr
{
    public class ParseTreeVisitor : ZephyrParserBaseVisitor<Node>
    {
        public override Node VisitProgram(ZephyrParser.ProgramContext context)
        {
            return Visit(context.statementList());
        }

        public override Node VisitStatementList(ZephyrParser.StatementListContext context)
        {
            
            //var nodes = context.children.Select(Visit).ToList();
            return new CompoundNode(GetBody(context));
        }

        public override Node VisitFuncDecl(ZephyrParser.FuncDeclContext context)
        {
            var idToken = new Token(TokenType.Id, context.Name.Text, context.Name.Column, context.Name.Line);

            var parameters = new List<Node>();
            if (context.funcParameters() is not null)
                parameters = GetBody(context.funcParameters());
            
            var type = context.Type?.GetText() ?? "void";
            var body = context.Body;

            return new FuncDeclNode(idToken, Visit(body), parameters, type);
        }

        public override Node VisitVarDecl(ZephyrParser.VarDeclContext context)
        {
            var decl = context.optionallyTypedVarDecl();
            var idToken = new Token(TokenType.Id, decl.Name.Text, decl.Name.Column, decl.Name.Line);
            TypeNode typeNode = null;
            if(decl.Type is not null)
            {
                var token = decl.Type.ID()?.Symbol ?? decl.Type.arrayType().ID().Symbol;
                var typeToken = new Token(
                    TokenType.Id, 
                    decl.Type.GetText(), 
                    token.Column, 
                    token.Column
                    );
                typeNode = new TypeNode(typeToken);
            }

            Node node = new VarDeclNode(new VarNode(idToken, true), typeNode);

            if (context.ASSIGN() is null) 
                return node;
            
            var assign = context.ASSIGN().Symbol;
            var assignToken = new Token(TokenType.Assign, assign.Text, assign.Column, assign.Line);
            return new BinOpNode(assignToken, node, Visit(context.equality()));
        }

        public override Node VisitTypedVarDecl(ZephyrParser.TypedVarDeclContext context)
        {
            var idToken = new Token(
                TokenType.Id, 
                context.Name.Text, 
                context.Name.Column,
                context.Name.Line);

            var token = context.Type.ID()?.Symbol ?? context.Type.arrayType().ID().Symbol;
            var typeToken = new Token(
                TokenType.Id, 
                context.Type.GetText(), 
                token.Column,
                token.Column);
            
            return new VarDeclNode(new VarNode(idToken, false),
                new TypeNode(typeToken));
        }

        public override Node VisitClassDecl(ZephyrParser.ClassDeclContext context)
        {
            var idToken = new Token(TokenType.Id, context.Name.Text, context.Name.Column, context.Name.Line);
            VarNode parentNode = null;
            if(context.Base is not null)
            {
                var parentToken = new Token(TokenType.Id, context.Base.Text, context.Base.Column, context.Base.Line);
                parentNode = new VarNode(parentToken, false);
            }

            var body = GetBody(context.classBody());
            return new ClassNode(idToken, parentNode, body);
        }

        public override Node VisitOptionallyTypedVarDecl(ZephyrParser.OptionallyTypedVarDeclContext context)
        {
            var varToken = new Token(TokenType.Id, context.Name.Text, context.Name.Column, context.Name.Line);
            TypeNode typeNode = null;
            if (context.Type is null)
            {
                return new VarDeclNode(new VarNode(varToken, true), typeNode);
            }

            var token = context.Type.ID()?.Symbol ?? context.Type.arrayType().ID().Symbol;
            var typeToken = new Token(
                TokenType.Id,
                context.Type.GetText(),
                token.Column,
                token.Column);
            typeNode = new TypeNode(typeToken);

            return new VarDeclNode(new VarNode(varToken, true), typeNode);
        }

        public override Node VisitUseStmt(ZephyrParser.UseStmtContext context)
        {
            return new UseNode(context.@namespace().GetText());
        }

        public override Node VisitPrintStmt(ZephyrParser.PrintStmtContext context)
        {
            var printToken = context.PRINT().Symbol;
            var token = new Token(TokenType.Print, printToken.Text, printToken.Column, printToken.Line);

            return new UnOpNode(token, Visit(context.equality()));
        }

        public override Node VisitReturnStmt(ZephyrParser.ReturnStmtContext context)
        {
            var returnToken = context.RETURN().Symbol;
            var token = new Token(TokenType.Return, returnToken.Text, returnToken.Column, returnToken.Line);

            var expr = context.equality() is not null ? Visit(context.equality()) : null;
            return new ReturnNode(token, expr);
        }

        public override Node VisitCompound(ZephyrParser.CompoundContext context)
        {
            return new CompoundNode(GetBody(context.statementList()));
        }

        public override Node VisitIfStmt(ZephyrParser.IfStmtContext context)
        {
            var ifToken = context.IF().Symbol;
            var token = new Token(TokenType.If, ifToken.Text, ifToken.Column, ifToken.Line);

            var condition = Visit(context.Condition);
            var thenBlock = Visit(context.ThenBranch);
            var elseBlock = context.ELSE() is not null? Visit(context.ElseBranch) : null;
            return new IfNode(token, condition, thenBlock, elseBlock);
        }

        public override Node VisitWhileStmt(ZephyrParser.WhileStmtContext context)
        {
            var whileToken = context.WHILE().Symbol;
            var token = new Token(TokenType.While, whileToken.Text, whileToken.Column, whileToken.Line);

            var condition = Visit(context.Condition);
            var body = Visit(context.Body);

            return new WhileNode(token, condition, body);
        }

        public override Node VisitForStmt(ZephyrParser.ForStmtContext context)
        {
            var forToken = context.FOR().Symbol;
            var token = new Token(TokenType.Print, forToken.Text, forToken.Column, forToken.Line);

            Node initializer = new NoOpNode();
            if (context.Initializer is not null)
                initializer = Visit(context.Initializer);

            Node condition = null;
            if (context.Condition is not null)
                condition = Visit(context.Condition);

            Node postAction = new NoOpNode();
            if (context.PostAction is not null)
                postAction = Visit(context.PostAction);

            var body = Visit(context.Body);
            if (body is CompoundNode)
                body.GetChildren().Add(postAction);
            else
                body = new CompoundNode(new List<Node> {body, postAction});
            
            var loop = new WhileNode(token, condition, body);
            var result = new CompoundNode(new List<Node> {initializer, loop});

            return result;
        }

        public override Node VisitAssignExpr(ZephyrParser.AssignExprContext context)
        {
            var left = Visit(context.Lhs);
            if (context.ASSIGN() is null)
                return left;

            var assign = context.ASSIGN().Symbol;
            var assignToken = new Token(TokenType.Assign, assign.Text, assign.Column, assign.Line);
            left.SetLhs(true);
            return new BinOpNode(assignToken, left, Visit(context.Rhs));
        }
        
        public override Node VisitEquality(ZephyrParser.EqualityContext context)
        {
            if (context.Equality is not null)
            {
                return Visit(context.Equality);
            }
            
            if (context.factor() is not null)
                return Visit(context.factor());

            if (context.Expr is not null)
            {
                var expression = Visit(context.Expr);
                var index = Visit(context.Index);

                return new IndexNode(expression, index);
            }

            if (context.call() is not null)
            {
                Node node;
                var callee = context.ID().Symbol;
                if (context.Caller is not null)
                {
                    node = Visit(context.Caller);
                    node = new GetNode(new Token(TokenType.Id, callee.Text, callee.Column, callee.Line), node);
                }
                else
                {
                    node = new VarNode(new Token(TokenType.Id, callee.Text, callee.Column, callee.Line), false);
                }

                
                var call = context.call();
                
                var arguments = call.funcArguments() is not null ? GetBody(call.funcArguments()) : new List<Node>();
                //var getNode = new GetNode(new Token(TokenType.Id, callee.Text, callee.Column, callee.Line), node);
                return new FuncCallNode(node, node.Token, arguments);
            }
            
            if (context.Caller is not null)
            {
                var callee = context.ID().Symbol;
                var calleeToken = new Token(TokenType.Id, callee.Text, callee.Column, callee.Line);
                return new GetNode(calleeToken, Visit(context.Caller));
            }

            if (context.Callee is not null)
            {
                var callee = context.ID().Symbol;
                return new VarNode(new Token(TokenType.Id, callee.Text, callee.Column, callee.Line), false);
            }

            var op = context.Op;
            var token = op.Text switch
            {
                "==" => new Token(TokenType.Equal, "==", op.Column, op.Line),
                "!=" => new Token(TokenType.NotEqual, "!=", op.Column, op.Line),
                "<" => new Token(TokenType.Less, "<", op.Column, op.Line),
                "<=" => new Token(TokenType.LessEqual, "!=", op.Column, op.Line),
                ">" => new Token(TokenType.Greater, ">", op.Column, op.Line),
                ">=" => new Token(TokenType.GreaterEqual, ">=", op.Column, op.Line),
                "+" => new Token(TokenType.Plus, "+", op.Column, op.Line),
                "-" => new Token(TokenType.Minus, "-", op.Column, op.Line),
                "*" => new Token(TokenType.Multiply, "*", op.Column, op.Line),
                "/" => new Token(TokenType.Divide, "/", op.Column, op.Line)
            };
            
            return new BinOpNode(token, Visit(context.Left), Visit(context.Right));
        }

        public override Node VisitFactor(ZephyrParser.FactorContext context)
        {
            if (context.Op is not null)
            {
                var op = context.Op;
                var token = op.Text switch
                {
                    "+" => new Token(TokenType.Plus, "+", op.Column, op.Line),
                    "-" => new Token(TokenType.Minus, "-", op.Column, op.Line),
                    "!" => new Token(TokenType.Not, "!", op.Column, op.Line),
                    _ => null
                };
                
                return new UnOpNode(token, Visit(context.factor()));
            }

            return Visit(context.primary());
        }

        public override Node VisitArrayInitializer(ZephyrParser.ArrayInitializerContext context)
        {
            var elements = GetBody(context);
            return new ArrayInitializerNode(elements);
        }

        public override Node VisitPrimary(ZephyrParser.PrimaryContext context)
        {
            if (context.literal() is not null)
            {
                return Visit(context.literal());
            }

            if (context.ifStmt() is not null)
            {
                return Visit(context.ifStmt());
            }

            if (context.compound() is not null)
            {
                return Visit(context.compound());
            }

            if (context.arrayInitializer() is not null)
            {
                return Visit(context.arrayInitializer());
            }

            if (context.@namespace() is not null)
            {
                return Visit(context.@namespace());
            }

            if (context.equality() is not null)
            {
                return Visit(context.equality());
            }

            var id = context.ID().Symbol;
            var token = new Token(TokenType.Id, id.Text, id.Line, id.Line);

            return new VarNode(token, false);
        }

        public override Node VisitNamespace(ZephyrParser.NamespaceContext context)
        {
            var names = context.GetText();
            return new VarNode(new Token(TokenType.Id, names, context.Start.Column, context.Start.Line), false);
        }

        public override Node VisitLiteral(ZephyrParser.LiteralContext context)
        {
            Token token = null;
            if (context.Int is not null)
                token = new Token(TokenType.Integer, int.Parse(context.Int.Text), 
                    context.Int.Column, context.Int.Line);
            
            else if (context.StringLit is not null)
                token = new Token(TokenType.StringLit, context.StringLit.Text.Trim('"'),
                    context.StringLit.Line, context.StringLit.Line);
            
            else if (context.Float is not null)
                token = new Token(TokenType.DoubleLit, 
                    double.Parse(context.Float.Text, CultureInfo.InvariantCulture), 
                    context.Float.Column, context.Float.Line);
            
            else if (context.True is not null)
                token = new Token(TokenType.True, bool.Parse(context.True.Text),
                    context.True.Column, context.True.Line);
            
            else if (context.False is not null)
                token = new Token(TokenType.False, bool.Parse(context.False.Text),
                    context.False.Column, context.False.Line);

            return new LiteralNode(token, token.Value);
        }

        private List<Node> GetBody(IParseTree node)
        {
            var list = new List<Node>(node.ChildCount);
            for (var i = 0; i < node.ChildCount; i++)
            {
                var child = node.GetChild(i);
                if(child is ITerminalNode)
                    continue;
                
                list.Add(Visit(child));
            }

            return list;
        }

        protected override Node AggregateResult(Node aggregate, Node nextResult)
        {
            return nextResult is null ? aggregate : base.AggregateResult(aggregate, nextResult);
        }
    }
}
