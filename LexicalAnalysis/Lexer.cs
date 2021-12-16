using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Zephyr.LexicalAnalysis.Tokens;

namespace Zephyr.LexicalAnalysis
{
    public class Lexer
    {
        private string _code;
        private readonly LinkedList<Token> _tokens;
        private int _position;
        private int _line;
        private int _start;
        private int _current;
        private int _parNestingLevel;
        
        public Lexer(string code)
        {
            _code = code;
            _position = 1;
            _line = 1;
            _tokens = new LinkedList<Token>();
            _parNestingLevel = _start = _current = 0;
        }

        public void Analyze()
        {
            while (IsAtEnd() == false)
            {
                _start = _current;
                var c = Advance();
                switch (c)
                {
                    case ')':
                        _parNestingLevel--;
                        AddToken(TokenType.RPar); 
                        break;
                    case '(':
                        _parNestingLevel++;
                        AddToken(TokenType.LPar);
                        break;
                    case '{':
                        AddToken(TokenType.LBrace);
                        break;
                    case '}':
                        AddToken(TokenType.RBrace);
                        break;
                    case ',':
                        AddToken(TokenType.Comma);
                        break;
                    case '+':
                        AddToken(TokenType.Plus);
                        break;
                    case '-':
                        AddToken(TokenType.Minus);
                        break;
                    case '*':
                        AddToken(TokenType.Multiply);
                        break;
                    case '/':
                        if (Match('/'))
                            while (Peek() != '\n' && IsAtEnd() == false)
                                Advance();
                        else
                            AddToken(TokenType.Divide);
                        
                        break;
                    case ';':
                        AddToken(TokenType.Semicolon);
                        break;
                    case '.':
                        AddToken(TokenType.Dot);
                        break;
                    case ':':
                        AddToken(TokenType.Colon);
                        break;
                    case '!':
                        AddToken(Match('=') ? TokenType.NotEqual : TokenType.Not);
                        break;
                    case '=':
                        AddToken(Match('=') ? TokenType.Equal : Match('>') ? TokenType.Return : TokenType.Assign);
                        break;
                    case '<':
                        AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                        break;
                    case '>':
                        AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                        break;
                    case ' ':
                        GetTab();
                        break;
                    case '\r':
                        break;
                    case '\t':
                        if(_parNestingLevel == 0)
                            AddToken(TokenType.Tab);
                        break;
                    case '\n':
                        _line++;
                        _position = 1;
                        break;
                    case '"':
                        GetString();
                        break;
                    default:
                        if (char.IsDigit(c))
                            GetNumber();
                        else if (IsAlpha(c))
                            Identifier();
                        else
                            throw new LexerException($"Unexpected symbol at line {_line}");
                        break;
                }
            }
            
            AddToken(TokenType.Eof);
        }

        public LinkedList<Token> GetTokens() => _tokens;

        private void GetString()
        {
            while (Peek() != '"' && IsAtEnd() == false)
            {
                if (Peek() == '\n')
                {
                    _line++;
                    _position = 1;
                }

                Advance();
            }

            Advance();
            var value = _code[(_start + 1)..(_current - 1)];
            AddToken(TokenType.StringLit, value);
        }

        private void GetNumber()
        {
            while (char.IsDigit(Peek()))
                Advance();

            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                Advance();
                while (char.IsDigit(Peek()))
                    Advance();

                var value = _code[_start.._current];
                AddToken(TokenType.DoubleLit, double.Parse(value, CultureInfo.InvariantCulture));
                return;
            }

            var val = _code[_start.._current];
            AddToken(TokenType.Integer, int.Parse(val));
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()))
                Advance();

            var value = _code[_start.._current];
            var type = TokenType.Id;
            if (Reserved.Keywords.ContainsKey(value))
                type = Reserved.Keywords[value];
            
            AddToken(type, value);
        }

        private void GetTab()
        {
            _current--;
            int i = 0;
            int tabs = 0;
            while (Peek() == ' ')
            {
                Advance();
                i++;
                if (i % 4 == 0)
                    tabs++;
            }
            
            if(Peek() == '\n')
                return;

            for (int j = 0; j < tabs; j++)
            {
                AddToken(TokenType.Tab);
            }
        }

        private bool IsAlpha(char c)
        {
            return Regex.IsMatch(c.ToString(), "[A-Za-z_]");
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || char.IsDigit(c);
        }

        private char Advance()
        {
            return _code[_current++];
        }

        private void AddToken(TokenType type, object value = null)
        {
            _tokens.AddLast(new Token(type, value, _position, _line));
        }

        private bool Match(char expected)
        {
            if (IsAtEnd())
                return true;

            if (Peek() != expected)
                return false;

            _current++;
            return true;
        }

        private char Peek()
        {
            return IsAtEnd() ? '\0' : _code[_current];
        }

        private char PeekNext()
        {
            var pos = _current + 1;
            return pos > _code.Length ? '\0' : _code[pos];
        }

        private bool IsAtEnd()
        {
            return _current >= _code.Length;
        }
    }
}