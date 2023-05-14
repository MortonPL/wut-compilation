using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Helpers;
using VerboseCore.Interfaces;

namespace VerboseCore.Parser
{
    public partial class Parser
    {
        public static readonly TokenDictionary<UnaryOperatorType> UnaryOpDict = new()
        {
            { TokenType.ArithmeticSub,    UnaryOperatorType.ArithmeticNegate },
            { TokenType.KeywordNot,       UnaryOperatorType.LogicalNot },
            { TokenType.OperatorNoneTest, UnaryOperatorType.OperatorNoneTest },
        };
        public static UnaryOperatorType MapUnary(TokenType tt) => UnaryOpDict.GetValueOrDefault(tt, UnaryOperatorType.Any);

        public static readonly TokenDictionary<BinaryOperatorType> BinaryOpDict = new()
        {
            { TokenType.ArithmeticAdd,          BinaryOperatorType.ArithmeticAdd },
            { TokenType.ArithmeticSub,          BinaryOperatorType.ArithmeticSub },
            { TokenType.ArithmeticMul,          BinaryOperatorType.ArithmeticMul },
            { TokenType.ArithmeticDiv,          BinaryOperatorType.ArithmeticDiv },
            { TokenType.ArithmeticMod,          BinaryOperatorType.ArithmeticMod },
            { TokenType.KeywordAnd,             BinaryOperatorType.LogicalAnd },
            { TokenType.KeywordOr,              BinaryOperatorType.LogicalOr },
            { TokenType.ComparatorEqual,        BinaryOperatorType.ComparatorEqual },
            { TokenType.ComparatorNotEqual,     BinaryOperatorType.ComparatorNotEqual },
            { TokenType.ComparatorEqualText,    BinaryOperatorType.ComparatorEqualText },
            { TokenType.ComparatorNotEqualText, BinaryOperatorType.ComparatorNotEqualText },
            { TokenType.ComparatorLess,         BinaryOperatorType.ComparatorLess },
            { TokenType.ComparatorGreater,      BinaryOperatorType.ComparatorGreater },
            { TokenType.ComparatorLessEqual,    BinaryOperatorType.ComparatorLessEqual },
            { TokenType.ComparatorGreaterEqual, BinaryOperatorType.ComparatorGreaterEqual },
            { TokenType.OperatorConcatenate,    BinaryOperatorType.OperatorConcatenate },
            { TokenType.KeywordIs,              BinaryOperatorType.OperatorAssignment },
        };
        public static BinaryOperatorType MapBinary(TokenType tt) => BinaryOpDict.GetValueOrDefault(tt, BinaryOperatorType.Any);

        public static readonly TokenDictionary<VariableType> VarTypeDict = new()
        {
            { TokenType.KeywordNumber,  VariableType.Number },
            { TokenType.KeywordText,    VariableType.Text },
            { TokenType.KeywordFact,    VariableType.Fact },
            { TokenType.KeywordNothing, VariableType.Nothing },
        };
        public static VariableType MapVariableType(TokenType tt) => VarTypeDict.GetValueOrDefault(tt, VariableType.Any);

        public static TokenDictionary<JumpType> JumpTypeDict = new()
        {
            { TokenType.KeywordSkip,   JumpType.Skip },
            { TokenType.KeywordStop,   JumpType.Stop },
            { TokenType.KeywordReturn, JumpType.Return },
        };
        public JumpType MapJumpType(TokenType tt) => JumpTypeDict.GetValueOrDefault(tt, JumpType.Any);

        public static TokenDictionary<SymbolType> SymbolTypeDict = new()
        {
            { TokenType.Any, SymbolType.Any },
            { TokenType.ValueNumber, SymbolType.ValueNumber },
            { TokenType.ValueText, SymbolType.ValueText },
            { TokenType.ValueComment, SymbolType.ValueComment },
            { TokenType.ValueFact, SymbolType.ValueFact },
            { TokenType.ValueIdentifier, SymbolType.ValueIdentifier },
            { TokenType.OperatorDot, SymbolType.OperatorDot },
            { TokenType.OperatorComma, SymbolType.OperatorComma },
            { TokenType.OperatorTernaryYes, SymbolType.OperatorTernaryYes },
            { TokenType.OperatorTernaryNo, SymbolType.OperatorTernaryNo },
            { TokenType.OperatorParenthesisOpen, SymbolType.OperatorParenthesisOpen },
            { TokenType.OperatorParenthesisClose, SymbolType.OperatorParenthesisClose },
            { TokenType.OperatorConcatenate, SymbolType.OperatorConcatenate },
            { TokenType.OperatorNoneTest, SymbolType.OperatorNoneTest },
            { TokenType.ArithmeticAdd, SymbolType.ArithmeticAdd },
            { TokenType.ArithmeticSub, SymbolType.ArithmeticSub },
            { TokenType.ArithmeticMul, SymbolType.ArithmeticMul },
            { TokenType.ArithmeticDiv, SymbolType.ArithmeticDiv },
            { TokenType.ArithmeticMod, SymbolType.ArithmeticMod },
            { TokenType.ComparatorEqual, SymbolType.ComparatorEqual },
            { TokenType.ComparatorNotEqual, SymbolType.ComparatorNotEqual },
            { TokenType.ComparatorEqualText, SymbolType.ComparatorEqualText },
            { TokenType.ComparatorNotEqualText, SymbolType.ComparatorNotEqualText },
            { TokenType.ComparatorLess, SymbolType.ComparatorLess },
            { TokenType.ComparatorGreater, SymbolType.ComparatorGreater },
            { TokenType.ComparatorLessEqual, SymbolType.ComparatorLessEqual },
            { TokenType.ComparatorGreaterEqual, SymbolType.ComparatorGreaterEqual },
            { TokenType.KeywordAnd, SymbolType.KeywordAnd },
            { TokenType.KeywordBegin, SymbolType.KeywordBegin },
            { TokenType.KeywordCall, SymbolType.KeywordCall },
            { TokenType.KeywordDefault, SymbolType.KeywordDefault },
            { TokenType.KeywordDo, SymbolType.KeywordDo },
            { TokenType.KeywordElse, SymbolType.KeywordElse },
            { TokenType.KeywordEnd, SymbolType.KeywordEnd },
            { TokenType.KeywordFact, SymbolType.KeywordFact },
            { TokenType.KeywordFalse, SymbolType.KeywordFalse },
            { TokenType.KeywordFor, SymbolType.KeywordFor },
            { TokenType.KeywordFunction, SymbolType.KeywordFunction },
            { TokenType.KeywordIs, SymbolType.KeywordIs },
            { TokenType.KeywordIf, SymbolType.KeywordIf },
            { TokenType.KeywordMatch, SymbolType.KeywordMatch },
            { TokenType.KeywordMutable, SymbolType.KeywordMutable },
            { TokenType.KeywordNone, SymbolType.KeywordNone },
            { TokenType.KeywordNow, SymbolType.KeywordNow },
            { TokenType.KeywordNot, SymbolType.KeywordNot },
            { TokenType.KeywordNothing, SymbolType.KeywordNothing },
            { TokenType.KeywordNumber, SymbolType.KeywordNumber },
            { TokenType.KeywordOr, SymbolType.KeywordOr },
            { TokenType.KeywordOtherwise, SymbolType.KeywordOtherwise },
            { TokenType.KeywordOverride, SymbolType.KeywordOverride },
            { TokenType.KeywordPattern, SymbolType.KeywordPattern },
            { TokenType.KeywordPipe, SymbolType.KeywordPipe },
            { TokenType.KeywordReturn, SymbolType.KeywordReturn },
            { TokenType.KeywordReturns, SymbolType.KeywordReturns },
            { TokenType.KeywordSkip, SymbolType.KeywordSkip },
            { TokenType.KeywordStop, SymbolType.KeywordStop },
            { TokenType.KeywordText, SymbolType.KeywordText },
            { TokenType.KeywordThen, SymbolType.KeywordThen },
            { TokenType.KeywordTrue, SymbolType.KeywordTrue },
            { TokenType.KeywordValue, SymbolType.KeywordValue },
            { TokenType.KeywordWhile, SymbolType.KeywordWhile },
            { TokenType.KeywordWith, SymbolType.KeywordWith },
        };
        public SymbolType MapSymbolType(TokenType tt) => SymbolTypeDict.GetValueOrDefault(tt, SymbolType.Any);
    }
}
