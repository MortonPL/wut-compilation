using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Helpers;

namespace VerboseCore.Lexer
{
    public partial class Lexer
    {
        public static readonly CharDictionary SingleOpDict = new()
        {
            { ';', TokenType.OperatorDot },
            { ',', TokenType.OperatorComma },
            { '?', TokenType.OperatorTernaryYes },
            { ':', TokenType.OperatorTernaryNo },
            { '(', TokenType.OperatorParenthesisOpen },
            { ')', TokenType.OperatorParenthesisClose },
            { '+', TokenType.ArithmeticAdd },
            { '-', TokenType.ArithmeticSub },
            { '*', TokenType.ArithmeticMul },
            { '/', TokenType.ArithmeticDiv },
            { '%', TokenType.ArithmeticMod },
            { '<', TokenType.ComparatorLess },
            { '>', TokenType.ComparatorGreater }
        };
        public static TokenType MapSignleOp(char c) => SingleOpDict.GetValueOrDefault(c, TokenType.Any);

        public static readonly CharDictionary<CharDictionary> DoubleOpDict = new()
        {
            { '=', new() { { '=', TokenType.ComparatorEqual } } },
            { '!', new() { { '=', TokenType.ComparatorNotEqual } } },
            { '<', new() { { '=', TokenType.ComparatorLessEqual } } },
            { '>', new() { { '=', TokenType.ComparatorGreaterEqual } } },
            { '+', new() { { '+', TokenType.OperatorConcatenate } } },
            { '?', new() { { '?', TokenType.OperatorNoneTest } } },
        };
        public static CharDictionary? MapDoubleOp(char c) => DoubleOpDict.GetValueOrDefault(c);

        public static readonly CharDictionary<CharDictionary<CharDictionary>> TripleOpDict = new()
        {
            { '=', new() { { '=', new() { { '=', TokenType.ComparatorEqualText } } } } },
            { '!', new() { { '=', new() { { '=', TokenType.ComparatorNotEqualText } } } } },
        };
        public static CharDictionary<CharDictionary>? MapTripleOp(char c) => TripleOpDict.GetValueOrDefault(c);

        public static readonly Dictionary<string, TokenType> KeywordDict = new()
        {
            { "AND", TokenType.KeywordAnd },
            { "BEGIN", TokenType.KeywordBegin },
            { "CALL", TokenType.KeywordCall },
            { "DEFAULT", TokenType.KeywordDefault },
            { "DO", TokenType.KeywordDo },
            { "ELSE", TokenType.KeywordElse },
            { "END", TokenType.KeywordEnd },
            { "FACT", TokenType.KeywordFact },
            { "FALSE", TokenType.KeywordFalse },
            { "FOR", TokenType.KeywordFor },
            { "FUNCTION", TokenType.KeywordFunction },
            { "IS", TokenType.KeywordIs },
            { "IF", TokenType.KeywordIf },
            { "MATCH", TokenType.KeywordMatch },
            { "MUTABLE", TokenType.KeywordMutable },
            { "NONE", TokenType.KeywordNone },
            { "NOW", TokenType.KeywordNow },
            { "NOT", TokenType.KeywordNot },
            { "NOTHING", TokenType.KeywordNothing },
            { "NUMBER", TokenType.KeywordNumber },
            { "OR", TokenType.KeywordOr },
            { "OTHERWISE", TokenType.KeywordOtherwise },
            { "OVERRIDE", TokenType.KeywordOverride },
            { "PATTERN", TokenType.KeywordPattern },
            { "PIPE", TokenType.KeywordPipe },
            { "RETURN", TokenType.KeywordReturn },
            { "RETURNS", TokenType.KeywordReturns },
            { "SKIP", TokenType.KeywordSkip },
            { "STOP", TokenType.KeywordStop },
            { "TEXT", TokenType.KeywordText },
            { "THEN", TokenType.KeywordThen },
            { "TRUE", TokenType.KeywordTrue },
            { "VALUE", TokenType.KeywordValue },
            { "WHILE", TokenType.KeywordWhile },
            { "WITH", TokenType.KeywordWith },
        };

        public static TokenType MapKeyword(string str) => KeywordDict.GetValueOrDefault(str, TokenType.Any);
    }
}
