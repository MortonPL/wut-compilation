using System.Collections.Generic;

using VerboseCore.Entities;

namespace VerboseCore.Helpers
{
    public class CharDictionary<T>: Dictionary<char, T> { }
    public class DualCharDictionary<T> : CharDictionary<CharDictionary<T>> { }
    public class TokenDictionary<T> : Dictionary<TokenType, T> { }

    public class CharDictionary: CharDictionary<TokenType> { }
}
