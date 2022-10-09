using System;
using System.Collections.Generic;
using System.Linq;
using Zephyr.LexicalAnalysis.Tokens;
using Zephyr.SyntaxAnalysis.ASTNodes;
using static Zephyr.LexicalAnalysis.Tokens.TokenType;

namespace Zephyr.SyntaxAnalysis
{
    public class Parser
    {
        private LinkedListNode<Token> _currentToken;
        private int _nestingLevel;
        
        public Parser(LinkedList<Token> tokens)
        {
            _currentToken = tokens.First;
            _nestingLevel = 0;
        }

        public Node Parse()
        {
            return Program();
        }

        private Node Program()
        {
            var nodes = StatementList();
            var eof = Match(Eof);
            if (eof is null && _currentToken.Value.Type == Tab)
                throw new ParserException(_currentToken.Value, "Expected colon at the beginning of a block");

            return new CompoundNode(nodes);
        }

        private Node Compound()
        {
            Require(Colon);
            _nestingLevel++;
            var node = new CompoundNode(StatementList());
            _nestingLevel--;
            Match(Semicolon);
            return node;
        }

        private List<Node> StatementList()
        {
            var nodes = new List<Node>();

            while (Match(Tab, _nestingLevel))
            {
                var node = Statement();
                nodes.Add(node);
                if(_currentToken.Value.Type == Eof)
                    break;

                Match(Semicolon);
            }

            return nodes;
        }

        private Node Statement()
        {
            if (_currentToken is null || _currentToken.Value.Type == Eof)
                return new NoOpNode();
            
            Ignore(Tab);

            return (_currentToken.Value.Type, _currentToken.Next.Value.Type) switch
            {
                (Id, Id) => Declaration(),
                (Class, _) => ClassDeclaration(),
                (Print, _) => PrintStatement(),
                (Return, _) => ReturnStatement(),
                (Colon, _) => Compound(),
                (If, _) => IfStatement(),
                (While, _) => WhileStatement(),
                (For, _) => ForStatement(),
                (Id, _) => AssignExpression()
            };
        }
        
        private Node ReturnStatement()
        {
            var token = Match(Return);
            if(Match(Semicolon) is null)
            {
                var node = AssignExpression();
                return new ReturnNode(token, node);
            }

            return new ReturnNode(token, null);
        }

        private Node Declaration()
        {
            return _currentToken.Next.Next.Value.Type switch
            {
                LPar => FuncDeclaration(),
                Colon => PropertyDeclaration(),
                _ => VarDeclaration()
            };
        }

        private Node ClassDeclaration()
        {
            Match(Class);
            var id = Require(Id);
            
            VarNode parent = null;
            if (Match(Less) is not null)
                parent = new VarNode(Require(Id));
            
            Require(Colon);
            _nestingLevel++;
            var body = ClassBody();
            _nestingLevel--;

            return new ClassNode(id, parent, body);
        }

        private List<Node> ClassBody()
        {
            var nodes = new List<Node>();
            
            while (Match(Tab, _nestingLevel))
            {
                var node = _currentToken.Value.Type == Class ? ClassDeclaration() : Declaration();
                nodes.Add(node);
                if(_currentToken.Value.Type == Eof)
                    break;
            }

            return nodes;
        }

        private Node VarDeclaration()
        {
            var typeToken = Match(Id);
            var idToken = Require(Id);
            Node node = new VarDeclNode(new VarNode(idToken), new TypeNode(typeToken));
            
            if (_currentToken.Value.Type == Assign)
                node = new BinOpNode(Match(Assign), node, Equality());

            return node;
        }

        private Node PropertyDeclaration()
        {
            var typeToken = Match(Id);
            var idToken = Require(Id);
            Match(Colon);
            Match(Tab, _nestingLevel + 1);
            _nestingLevel++;
            
            var getterToken = Match(Get);
            FuncDeclNode getterNode = null;
            
            if(getterToken is not null)
            {
                getterToken = getterToken with {Value = $"get_{idToken.Value}"};
                var getterBody = RiseNestingLevelIfNoColon();
                getterNode = new FuncDeclNode(getterToken, GetBody(getterBody), new List<Node>(), (string) typeToken.Value);
            }

            Match(Tab, _nestingLevel);
            
            var setterToken = Match(Set);
            FuncDeclNode setterNode = null;
            
            if(setterToken is not null)
            {
                setterToken = setterToken with {Value = $"set_{idToken.Value}"};
                var setterBody = RiseNestingLevelIfNoColon();
                
                var valueToken = new Token(Id, "value", setterToken.Position, setterToken.Line);
                var valueNode = new VarDeclNode(new VarNode(valueToken), new TypeNode(typeToken));
                
                setterNode = new FuncDeclNode(setterToken, GetBody(setterBody), new List<Node> {valueNode}, "void");
            }

            _nestingLevel--;

            var varNode = new VarDeclNode(new VarNode(idToken), new TypeNode(typeToken));

            return new PropertyDeclNode(varNode, getterNode, setterNode);
        }

        private Node FuncDeclaration()
        {
            var typeToken = Match(Id);
            var idToken = Require(Id);
            Match(LPar);
            var parameters = FuncParameters();
            Require(RPar);
            var body = RiseNestingLevelIfNoColon();
            return new FuncDeclNode(idToken, GetBody(body), parameters, (string)typeToken.Value);
        }

        private List<Node> FuncParameters()
        {
            var list = new List<Node>();
            if (_currentToken.Value.Type == RPar)
                return list;

            do
            {
                var node = VarDeclaration();
                list.Add(node);
            }
            while (Match(Comma) is not null);
            
            return list;
        }

        private Node PrintStatement()
        {
            var token = Match(Print);
            return new UnOpNode(token, AssignExpression());
        }

        private Node IfStatement()
        {
            var token = Match(If);
            
            var condition = OptionalParentheses(AssignExpression);
            
            var thenBlock = RiseNestingLevelIfNoColon();
            
            Node elseBlock = null;
            Match(Tab, _nestingLevel);
            if (Match(Else) is not null)
                elseBlock = RiseNestingLevelIfNoColon();

            return new IfNode(token, condition, thenBlock, elseBlock);
        }

        private Node WhileStatement()
        {
            var token = Match(While);
            var condition = OptionalParentheses(AssignExpression);
            var body = RiseNestingLevelIfNoColon();

            return new WhileNode(token, condition, body);
        }

        private Node ForStatement()
        {
            var token = Match(For);
            var par = Match(LPar);
            
            Node initializer = new NoOpNode();
            if(Match(Comma) is null)
            {
                if (_currentToken.Value.Type == Id && _currentToken.Next.Value.Type == Id)
                    initializer = VarDeclaration();
                else
                    initializer = Equality();
            }
            
            Require(Comma);
            Node condition = null;
            if(Match(Comma) is null)
                condition = Equality();
            
            condition ??= new LiteralNode(token, true);
            
            Require(Comma);
            Node postAction = new NoOpNode();
            if(Match(Comma) is null)
                postAction = AssignExpression();
            if (par is not null)
                Require(RPar);

            var body = RiseNestingLevelIfNoColon();
            if (body is CompoundNode)
                body.GetChildren().Add(postAction);
            else
                body = new CompoundNode(new() {body, postAction});
            
            var loop = new WhileNode(token, condition, body);
            var result = new CompoundNode(new() {initializer, loop});

            return result;
        }

        private Node AssignExpression()
        {
            var left = Equality();
            var assignToken = Match(Assign);
            if(assignToken is not null)
                return new BinOpNode(assignToken, left, AssignExpression());
            
            return left;
        }

        private Node Variable()
        {
            return new VarNode(Match(Id));
        }

        private Node Equality()
            => Operation(Comparison, Equal, NotEqual);

        private Node Comparison()
            => Operation(Expression, GreaterEqual, Greater, LessEqual, Less);

        private Node Expression()
            => Operation(Term, Plus, Minus);

        private Node Term()
            => Operation(Factor, Divide, Multiply);

        private Node Factor()
        {
            var unary = Match(Minus, Plus, Not);
            if (unary is not null)
                return new UnOpNode(unary, Factor());

            return Call();
        }
        
        private Node Call()
        {
            var node = Primary();
            while (true)
            {
                if(Match(LPar) is not null)
                {
                    if (Match(RPar) is not null)
                    {
                        node = new FuncCallNode(node, node.Token, new List<Node>());
                        continue;
                    }

                    var arguments = new List<Node> {Equality()};

                    while (Match(Comma) is not null)
                        arguments.Add(Equality());
                    Require(RPar);

                    node = new FuncCallNode(node, node.Token, arguments);
                }
                else if (Match(Dot) is not null)
                {
                    var token = Match(Id);
                    node = new GetNode(token, node);
                }
                else
                {
                    break;
                }
            }

            return node;
        }

        private Node Primary()
        {
            switch (_currentToken.Value.Type)
            {
                case StringLit or Integer or DoubleLit:
                    var token = Match(StringLit, Integer, DoubleLit);
                    return new LiteralNode(token, token.Value);
                case True or False:
                    token = Match(True, False);
                    return new LiteralNode(token, bool.Parse((string) token.Value));
                case Id:
                    return Variable();
                case LPar:
                    Match(LPar);
                    var res = Equality();
                    Require(RPar);
                    return res;
                default:
                    token = _currentToken.Value;
                    throw new ParserException(token, $"Unexpected token {token.Type}");
            }
        }

        private Node Operation(Func<Node> higherPrecedence, params TokenType[] operators)
        {
            var node = higherPrecedence();
            var token = Match(operators);
            while (token is not null)
            {
                node = new BinOpNode(token, node, higherPrecedence());
                token = Match(operators);
            }

            return node;
        }

        private Token Match(TokenType type)
        {
            var token = _currentToken?.Value;
            
            if (token is null || token.Type != type)
                return null;

            _currentToken = _currentToken.Next;
            
            return token;
        }

        private bool Match(TokenType type, int times)
        {
            for (int i = 0; i < times; i++)
            {
                if (Match(type) is not null) 
                    continue;
                
                StepBack(i);
                return false;
            }

            if (Match(type) is null) 
                return true;
            
            StepBack();
            return false;

        }

        private Token Match(params TokenType[] types)
        {
            var token = _currentToken?.Value;
            
            if (token is null || types.All(type => token.Type != type))
                return null;
            
            token = _currentToken.Value;
            _currentToken = _currentToken.Next;

            return token;
        }

        private Token Require(TokenType type)
        {
            var token = Match(type);
            if (token is not null)
                return token;

            var current = _currentToken.Value;
            throw new ParserException(current, $"Unexpected token {current.Type}, expected: {type}");
        }

        private Token Require(params TokenType[] types)
        {
            var token = Match(types);
            if (token is not null)
                return token;

            var current = _currentToken.Value;
            throw new ParserException(current, $"Unexpected token {current.Type}");
        }

        private void StepBack(int times = 1)
        {
            for (int i = 0; i < times; i++)
                _currentToken = _currentToken.Previous;
        }

        private Node RiseNestingLevelIfNoColon()
        {
            var colon = Match(Colon);
            if (colon is null)
                _nestingLevel++;
            else
                StepBack();
            
            var result = Statement();
            
            if (colon is null)
                _nestingLevel--;

            return result;
        }

        private Node OptionalParentheses(Func<Node> op)
        {
            var par = Match(LPar);
            var result = op();
            if (par is not null)
                Require(RPar);

            return result;
        }

        private void Ignore(TokenType type)
        {
            while (Match(type) is not null)
            { }
        }

        private List<Node> GetBody(Node n)
        {
            if (n is CompoundNode)
                return n.GetChildren();

            return new() {n};
        }
    }
}