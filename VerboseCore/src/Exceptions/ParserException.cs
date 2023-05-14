using System;
using System.Collections.Generic;

using VerboseCore.Interfaces;

namespace VerboseCore.Exceptions
{
    public class ParserError : Exception
    {
        public ErrorType Error;
        public List<object> Values;

        public ParserError(ErrorType type) : base()
        {
            Error = type;
            Values = new();
        }
        public ParserError(ErrorType type, List<object> values) : base()
        {
            Error = type;
            Values = values;
        }
    }
}
