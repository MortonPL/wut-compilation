using System;
using System.Collections.Generic;

using VerboseCore.Interfaces;

namespace VerboseCore.Exceptions
{
    public class LexerError : Exception
    {
        public ErrorType Error;
        public List<object> Values;

        public LexerError(ErrorType type) : base()
        {
            Error = type;
            Values = new();
        }
        public LexerError(ErrorType type, List<object> values) : base()
        {
            Error = type;
            Values = values;
        }
    }
}
