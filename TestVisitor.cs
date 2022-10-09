using System;
using System.Collections.Generic;
using System.Globalization;
using Antlr4.Runtime.Tree;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SyntaxAnalysis.ASTNodes;

namespace Zephyr
{
    public class TestVisitor : testBaseVisitor<Node>
    {
        public override Node VisitProgram(test.ProgramContext context)
        {
            return Visit(context.statementList());
        }

        public override Node VisitStatementList(test.StatementListContext context)
        {
            var nodes = new List<Node>();
            foreach (var node in context.children)
            {
                nodes.Add(Visit(node));
            }
            return new CompoundNode(nodes);
        }

        public override Node VisitFuncDecl(test.FuncDeclContext context)
        {
            
            var idToken = new Token(TokenType.Id, 
                context.ID().GetText(), 
                context.ID().Symbol.Column,
                context.ID().Symbol.Line);

            List<Node> parameters = new List<Node>();
            if (context.funcParameters() is not null)
                parameters = GetBody(context.funcParameters());
            string type = "void";
            if (context.type() is not null)
                type = context.type().GetText();
            return new FuncDeclNode(idToken,
                GetBody(context.statementList()), 
                parameters,
                type);
        }

        public override Node VisitVarDecl(test.VarDeclContext context)
        {
            var idToken = new Token(TokenType.Id, 
                context.ID().GetText(), 
                context.ID().Symbol.Column,
                context.ID().Symbol.Line);
            var typeToken = new Token(TokenType.Id, 
                context.type().GetText(), 
                context.type().ID().Symbol.Column,
                context.type().ID().Symbol.Line);

            Node node = new VarDeclNode(new VarNode(idToken),
                new TypeNode(typeToken));
            ITerminalNode assign;
            if ((assign = context.ASSIGN()) is not null)
            {
                node = new BinOpNode(new Token(TokenType.Assign, assign.GetText(),
                        assign.Symbol.Column, assign.Symbol.Line),
                    node, Visit(context.assignExpr()));
            }

            return node;
        }

        public override Node VisitTypedVarDecl(test.TypedVarDeclContext context)
        {
            var idToken = new Token(TokenType.Id, 
                context.ID().GetText(), 
                context.ID().Symbol.Column,
                context.ID().Symbol.Line);
            var typeToken = new Token(TokenType.Id, 
                context.type().GetText(), 
                context.type().ID().Symbol.Column,
                context.type().ID().Symbol.Line);
            return new VarDeclNode(new VarNode(idToken),
                new TypeNode(typeToken));
        }

        public override Node VisitClassDecl(test.ClassDeclContext context)
        {
            var idToken = new Token(TokenType.Id, 
                context.ID(0).GetText(), 
                context.ID(0).Symbol.Column,
                context.ID(0).Symbol.Line);
            VarNode parentNode = null;
            if(context.ID(1) is not null)
            {
                var parentToken = new Token(TokenType.Id,
                    context.ID(1).GetText(),
                    context.ID(1).Symbol.Column,
                    context.ID(1).Symbol.Line);
                parentNode = new VarNode(parentToken);
            }

            var body = GetBody(context.classBody());
            return new ClassNode(idToken, parentNode, body);
        }

        public override Node VisitPrintStmt(test.PrintStmtContext context)
        {
            var token = new Token(TokenType.Print, 
                context.PRINT().GetText(), 
                context.PRINT().Symbol.Column,
                context.PRINT().Symbol.Line);

            return new UnOpNode(token, Visit(context.assignExpr()));
        }

        public override Node VisitReturnStmt(test.ReturnStmtContext context)
        {
            var token = new Token(TokenType.Return, 
                context.RETURN().GetText(), 
                context.RETURN().Symbol.Column,
                context.RETURN().Symbol.Line);

            var expr = context.assignExpr() is not null ? Visit(context.assignExpr()) : null;
            return new ReturnNode(token, expr);
        }

        public override Node VisitCompound(test.CompoundContext context)
        {
            return new CompoundNode(GetBody(context.statementList()));
        }

        public override Node VisitIfStmt(test.IfStmtContext context)
        {
            var token = new Token(TokenType.If, 
                context.IF().GetText(), 
                context.IF().Symbol.Column,
                context.IF().Symbol.Line);

            var condition = Visit(context.assignExpr());
            var thenBlock = Visit(context.statement(0));
            var elseBlock = context.ELSE() is not null? Visit(context.statement(1)) : null;
            return new IfNode(token, condition, thenBlock, elseBlock);
        }

        public override Node VisitWhileStmt(test.WhileStmtContext context)
        {
            var token = new Token(TokenType.While, 
                context.WHILE().GetText(), 
                context.WHILE().Symbol.Column,
                context.WHILE().Symbol.Line);

            var condition = Visit(context.assignExpr());
            var body = Visit(context.statement());

            return new WhileNode(token, condition, body);
        }

        public override Node VisitForStmt(test.ForStmtContext context)
        {
            var token = new Token(TokenType.Print, 
                context.FOR().GetText(), 
                context.FOR().Symbol.Column,
                context.FOR().Symbol.Line);

            Node initializer = new NoOpNode();
            if (context.varDecl() is not null)
                initializer = Visit(context.varDecl());

            Node condition = null;
            if (context.equality() is not null)
                condition = Visit(context.equality());

            Node postAction = new NoOpNode();
            if (context.assignExpr() is not null)
                postAction = Visit(context.assignExpr());

            var body = Visit(context.statement());
            if (body is CompoundNode)
                body.GetChildren().Add(postAction);
            else
                body = new CompoundNode(new() {body, postAction});
            
            var loop = new WhileNode(token, condition, body);
            var result = new CompoundNode(new() {initializer, loop});

            return result;
        }

        public override Node VisitAssignExpr(test.AssignExprContext context)
        {
            var left = Visit(context.equality());
            ITerminalNode assign;
            if ((assign = context.ASSIGN()) is not null)
            {
                var assignToken = new Token(TokenType.Assign, assign.GetText(),
                    assign.Symbol.Column, assign.Symbol.Line);
                return new BinOpNode(assignToken, left, Visit(context.assignExpr()));
            }

            return left;
        }

        public override Node VisitEquality(test.EqualityContext context)
        {
            var node = Visit(context.comparison(0));
            Token token = null;
            var children = context.children;
            for(int i = 0; i < children.Count; i++)
            {
                var child = context.GetChild(i);
                if (child is ITerminalNode terminal)
                {
                    token = terminal.Symbol.Text switch
                    {
                        "==" => new Token(TokenType.Equal, "==", terminal.Symbol.Column, terminal.Symbol.Line),
                        "!=" => new Token(TokenType.NotEqual, "!=", terminal.Symbol.Column, terminal.Symbol.Line)
                    };
                    node = new BinOpNode(token, node, Visit(context.GetChild(++i)));
                }
            }

            return node;
        }

        public override Node VisitComparison(test.ComparisonContext context)
        {
            var node = Visit(context.expression(0));
            Token token = null;
            var children = context.children;
            for(int i = 0; i < children.Count; i++)
            {
                var child = context.GetChild(i);
                if (child is ITerminalNode terminal)
                {
                    token = terminal.Symbol.Text switch
                    {
                        "<" => new Token(TokenType.Less, "<", terminal.Symbol.Column, terminal.Symbol.Line),
                        "<=" => new Token(TokenType.LessEqual, "!=", terminal.Symbol.Column, terminal.Symbol.Line),
                        ">" => new Token(TokenType.Greater, ">", terminal.Symbol.Column, terminal.Symbol.Line),
                        ">=" => new Token(TokenType.GreaterEqual, ">=", terminal.Symbol.Column, terminal.Symbol.Line)
                    };
                    node = new BinOpNode(token, node, Visit(context.GetChild(++i)));
                }
            }

            return node;
        }

        public override Node VisitExpression(test.ExpressionContext context)
        {
            var node = Visit(context.term(0));
            Token token = null;
            var children = context.children;
            for(int i = 0; i < children.Count; i++)
            {
                var child = context.GetChild(i);
                if (child is ITerminalNode terminal)
                {
                    token = terminal.Symbol.Text switch
                    {
                        "+" => new Token(TokenType.Plus, "+", terminal.Symbol.Column, terminal.Symbol.Line),
                        "-" => new Token(TokenType.Minus, "-", terminal.Symbol.Column, terminal.Symbol.Line)
                    };
                    node = new BinOpNode(token, node, Visit(context.GetChild(++i)));
                }
            }

            return node;
        }

        public override Node VisitTerm(test.TermContext context)
        {
            var node = Visit(context.factor(0));
            Token token = null;
            var children = context.children;
            for(int i = 0; i < children.Count; i++)
            {
                var child = context.GetChild(i);
                if (child is ITerminalNode terminal)
                {
                    token = terminal.Symbol.Text switch
                    {
                        "*" => new Token(TokenType.Multiply, "*", terminal.Symbol.Column, terminal.Symbol.Line),
                        "/" => new Token(TokenType.Divide, "/", terminal.Symbol.Column, terminal.Symbol.Line)
                    };

                    node = new BinOpNode(token, node, Visit(child));
                }
            }

            return node;
        }

        public override Node VisitFactor(test.FactorContext context)
        {
            Token token = null;
            if(context.PLUS() is not null)
                token = new Token(TokenType.Plus, 
                    context.PLUS().GetText(), 
                    context.PLUS().Symbol.Column,
                    context.PLUS().Symbol.Line);
            else if(context.MINUS() is not null)
                token = new Token(TokenType.Minus, 
                    context.MINUS().GetText(), 
                    context.MINUS().Symbol.Column,
                    context.MINUS().Symbol.Line);
            else if(context.NOT() is not null)
                token = new Token(TokenType.Not, 
                    context.NOT().GetText(), 
                    context.NOT().Symbol.Column,
                    context.NOT().Symbol.Line);

            if (token is not null)
                return new UnOpNode(token, Visit(context.factor()));

            return Visit(context.call());
        }

        public override Node VisitCall(test.CallContext context)
        {
            var node = Visit(context.primary());
            var children = context.children;
            for (int i = 0; i < children.Count; i++)
            {
                if (context.GetChild(i) is ITerminalNode terminal)
                {
                    if (terminal.Symbol.Text == "(")
                    {
                        var args = context.GetChild(++i);
                        List<Node> arguments = new List<Node>();
                        if(args is not null)
                        {
                            arguments = GetBody(args);
                        }
                        node = new FuncCallNode(node, node.Token, arguments);
                    }
                    else if (terminal.Symbol.Text == ".")
                    {
                        var id = context.GetChild(++i);
                        if(id is ITerminalNode term)
                        {
                            var token = new Token(TokenType.Id, term.GetText(), term.Symbol.Column, term.Symbol.Line);
                            node = new GetNode(token, node);
                        }
                    }
                }
            }

            return node;
        }

        public override Node VisitPrimary(test.PrimaryContext context)
        {
            if (context.literal() is not null)
            {
                return Visit(context.literal());
            }
            
            if (context.ID() is not null)
            {
                var token = new Token(TokenType.StringLit,
                    context.ID().GetText(),
                    context.ID().Symbol.Line,
                    context.ID().Symbol.Line);

                return new VarNode(token);
            }

            return Visit(context.equality());
        }

        public override Node VisitLiteral(test.LiteralContext context)
        {
            var lit = context;
            Token token = null;
            if (lit.INT() is not null)
                token = new Token(TokenType.Integer,
                    int.Parse(lit.INT().GetText()), 
                    lit.INT().Symbol.Column,
                    lit.INT().Symbol.Line);
            else if (lit.STRING_LITERAL() is not null)
                token = new Token(TokenType.StringLit,
                    lit.STRING_LITERAL().GetText().Trim('"'),
                    lit.STRING_LITERAL().Symbol.Line,
                    lit.STRING_LITERAL().Symbol.Line);
            else if (lit.FLOAT() is not null)
                token = new Token(TokenType.DoubleLit,
                    double.Parse(lit.FLOAT().GetText(), CultureInfo.InvariantCulture),
                    lit.FLOAT().Symbol.Column,
                    lit.FLOAT().Symbol.Line);
            else if (lit.TRUE() is not null)
                token = new Token(TokenType.True,
                    bool.Parse(lit.TRUE().GetText()),
                    lit.TRUE().Symbol.Column,
                    lit.TRUE().Symbol.Line);
            else if (lit.FALSE() is not null)
                token = new Token(TokenType.False,
                    bool.Parse(lit.FALSE().GetText()),
                    lit.FALSE().Symbol.Column,
                    lit.FALSE().Symbol.Line);


            return new LiteralNode(token, token.Value);
        }

        private List<Node> GetBody(IParseTree node)
        {
            var list = new List<Node>(node.ChildCount);
            for (int i = 0; i < node.ChildCount; i++)
            {
                if(node.GetChild(i) is ITerminalNode)
                    continue;
                
                list.Add(Visit(node.GetChild(i)));
            }

            return list;
        }
    }
}