using System;
using System.IO;

using VerboseCore.Lexer;

namespace VerboseCore.Helpers
{
    public static class Helpers
    {
        public static Action<T> GetNoAction<T>() => _ => { };

        public static Lexer.Lexer MakeStringLexer(string source)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(source);
            writer.Flush();
            stream.Position = 0;
            return new Lexer.Lexer(new LexerScanner(stream), new MockLogger());
        }
    }
}
