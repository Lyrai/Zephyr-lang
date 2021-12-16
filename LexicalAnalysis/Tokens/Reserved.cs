using System.Collections.Generic;

namespace Zephyr.LexicalAnalysis.Tokens
{
    public static class Reserved
    {
        public static readonly Dictionary<string, TokenType> Keywords = new()
        {
            {"int", TokenType.Id},
            {"double", TokenType.Id},
            {"bool", TokenType.Id},
            {"string", TokenType.Id},
            {"void", TokenType.Id},
            {"function", TokenType.Id},
            {"return", TokenType.Return},
            {"print", TokenType.Print},
            {"true", TokenType.True},
            {"false", TokenType.False},
            {"get", TokenType.Get},
            {"set", TokenType.Set},
            {"if", TokenType.If},
            {"else", TokenType.Else},
            {"while", TokenType.While},
            {"for", TokenType.For},
            {"class", TokenType.Class}
        };
    }
}