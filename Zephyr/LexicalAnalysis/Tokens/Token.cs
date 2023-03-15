using Zephyr.LexicalAnalysis.Tokens;

namespace Zephyr
{
    public record Token
    {
        public TokenType Type { get; init; }
        public object Value { get; init; }
        public int Position { get; init; }
        public int Line { get; init; }

        public Token(TokenType type, object value, int position, int line)
        {
            Type = type;
            Value = value;
            Position = position;
            Line = line;
        }
    }
}