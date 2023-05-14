using VerboseCore.Abstract;
using VerboseCore.Entities;

namespace VerboseCore.Interfaces
{
    public interface IScanner<T>
    {
        public APosition Position { get; }
        public T Buffer { get; }
        public string BufferedError { get; }

        public void Next();
    }

    public interface ILexerScanner : IScanner<char>
    {
        public void ClearBufferedError();
        public void NextCharInString();
        public void NextCharInNumber();
        public bool TryNewline();
        public void SkipWhites();
        public char HandleEscape();
        public bool IsSpecialWhiteSpace();
    }

    public interface IParserScanner : IScanner<IToken>
    {
        public TokenType Type { get; }
        public object? Value { get; }
    }
}
