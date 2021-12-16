namespace Zephyr
{
    public class ParserException : GeneralException
    {
        public ParserException(Token token, string message) : base($"{message} at line {token.Line}")
        { }
    }
}