using VerboseCore.Abstract;
using VerboseCore.Entities;
using VerboseCore.Interfaces;

namespace VerboseCore.Parser
{
    public class ParserScanner: IParserScanner
    {
        private readonly ILexer _lexer;

        public APosition Position { get => Buffer.Position; }
        public IToken Buffer { get; private set; }
        public string BufferedError { get => _lexer.BufferedError; }
        public TokenType Type { get => Buffer.Type; }
        public object? Value { get => Buffer.Value; }

        public ParserScanner(ILexer lexer)
        {
            _lexer = lexer;
            Buffer = new Token(TokenType.Any, new Position());
            Next();
        }

        public void Next()
        {
            Buffer = _lexer.BuildToken();
            if (Buffer.Type == TokenType.SpecialEOL ||
                Buffer.Type == TokenType.ValueComment)
                Next();
        }
    }
}
