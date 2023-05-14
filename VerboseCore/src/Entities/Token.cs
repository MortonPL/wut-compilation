using VerboseCore.Abstract;
using VerboseCore.Interfaces;

namespace VerboseCore.Entities
{
    public class Token: IToken
    {
        public TokenType Type { get; set; }
        public APosition Position { get; set; }
        public object? Value { get; set; }

        public Token(TokenType type, APosition position, object? value = null)
        {
            Type = type;
            Position = position;
            Value = value;
        }
    }
}
