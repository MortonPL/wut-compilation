using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

using System;

using VerboseCore.Helpers;
using VerboseCore.Exceptions;
using VerboseCore.Lexer;
using VerboseCore.Interfaces;

using TT = VerboseCore.Entities.TokenType;

namespace VerboseTests.Lexer_
{
    [TestClass]
    public class LexerTests
    {
        static readonly Action<IToken> NoAction = _ => { };

        static private Lexer PrepareLexer(string str)
        {
            return Helpers.MakeStringLexer(str);
        }

        static private IToken TestCore(Lexer lexer, TT expectedType,
            Action<IToken> extraActions, Action<IToken> failActions)
        {
            var token = lexer.BuildToken();
            if (token.Type == expectedType)
            {
                token.Type.Should().Be(expectedType);
                extraActions(token);
            }
            else
            {
                failActions(token);
                token.Type.Should().Be(expectedType, $"Unexpected Value: {token.Value}");
            }
            return token;
        }

        // Test if the type is matching (constant) and if the value matches expectations.
        static private IToken TestWithValue(Lexer lexer, TT tt, object correct)
        {
            return TestCore(lexer, tt,
                t => t.Value.Should().Be(correct),
                NoAction);
        }

        // Test if the type is matching.
        static private IToken TestWithType(Lexer lexer, TT tt)
        {
            return TestCore(lexer, tt, NoAction, NoAction);
        }

        static private void TestWithException(Lexer lexer, ErrorType type)
        {
            Action a = () =>lexer.BuildToken();

            a.Should().Throw<LexerError>().Where(e => e.Error == type);
        }

        [TestMethod]
        [DataRow("      0",                     TT.ValueNumber,      0.0)]
        [DataRow("1234567890",                  TT.ValueNumber,      1234567890.0)]
        [DataRow("1_000_200",                   TT.ValueNumber,      1_000_200.0)]
        [DataRow("1234.567890",                 TT.ValueNumber,      1234.56789)]
        [DataRow("0.123456789",                 TT.ValueNumber,      0.123456789)]
        [DataRow("0.12_345",                    TT.ValueNumber,      0.12_345)]
        [DataRow("12.34.56.789",                TT.ValueNumber,      12.34)]
        [DataRow("1_________",                  TT.ValueNumber,      1.0)]
        [DataRow("_456_",                       TT.ValueIdentifier,  "_456_")]
        [DataRow("0b1010",                      TT.ValueNumber,      (double)0b1010)]
        [DataRow("0b0101",                      TT.ValueNumber,      (double)0b0101)]
        [DataRow("0b01.01",                     TT.ValueNumber,      1.0)]
        [DataRow("0b11011010_00001111",         TT.ValueNumber,      (double)0b11011010_00001111)]
        [DataRow("0o123",                       TT.ValueNumber,      83.0)]
        [DataRow("0x1b3",                       TT.ValueNumber,      (double)0x1b3)]
        [DataRow("0xAaCc",                      TT.ValueNumber,      (double)0xaacc)]
        [DataRow("0xAF_96_2E",                  TT.ValueNumber,      (double)0xAF_96_2E)]
        [DataRow("a457",                        TT.ValueIdentifier,  "a457")]
        [DataRow("10 2389",                     TT.ValueNumber,      10.0)]
        [DataRow("1{000}{000}",                 TT.ValueNumber,      1000000.0)]
        [DataRow("{}69_{}.}}4__}{{2_0__}{_",    TT.ValueNumber,      69.420)]
        [DataRow("1 0",                         TT.ValueNumber,      1.0)]
        public void TestNumber(string str, TT tt, object correct)
        {
            var lexer = PrepareLexer(str);
            TestWithValue(lexer, tt, correct);
        }

        [TestMethod]
        [DataRow("0123456789", ErrorType.NumberNotADecimal)]
        [DataRow("345.. 678", ErrorType.NumberNotADecimal)]
        [DataRow("0b.0101", ErrorType.NumberNotInBase)]
        [DataRow("0b", ErrorType.NumberNotInBase)]
        [DataRow("0c1", ErrorType.NumberUnknownBase)]
        [DataRow("99999999999999999999999999999999999999999999" +
            "9999999999999999999999999999999999999999999999999999999999999999999" +
            "9999999999999999999999999999999999999999999999999999999999999999999" +
            "9999999999999999999999999999999999999999999999999999999999999999999" +
            "9999999999999999999999999999999999999999999999999999999999999999999",
            ErrorType.NumberOverflow)]
        public void TestNumberException(string str, ErrorType type)
        {
            var lexer = PrepareLexer(str);
            TestWithException(lexer, type);
        }

        [TestMethod]
        [DataRow("\"abba\"",                            TT.ValueText,        "abba")]
        [DataRow("\"jeden\ndwa\ntrzy\"",                TT.ValueText,        "jeden\ndwa\ntrzy")]
        [DataRow("\"AC\\\\DC\"",                        TT.ValueText,        "AC\\DC")]
        [DataRow("\"ala ma kota też.\"",                TT.ValueText,        "ala ma kota też.")]
        [DataRow("\"\\\"\\\\\"",                        TT.ValueText,        "\"\\")]
        [DataRow("\"\\a\\b\\f\\n\\r\\t\\v\\\\\\\"\\\'\"", TT.ValueText,      "\a\b\f\n\r\t\v\\\"\'")]
        [DataRow("never???story",                       TT.ValueIdentifier,  "never")]
        [DataRow("neverstartingstory\"",                TT.ValueIdentifier,  "neverstartingstory")]
        [DataRow("\"\"",                                TT.ValueText,        "")]
        [DataRow("\"   \n\t \"",                        TT.ValueText,        "   \n\t ")]
        [DataRow("\"kla{mry}\"",                        TT.ValueText,        "kla{mry}")]
        [DataRow("\"冰淇淋\"",                           TT.ValueText,        "冰淇淋")]
        [DataRow("\"sadeg 😔\"",                        TT.ValueText,        "sadeg 😔")]
        [DataRow("\"####\"",                            TT.ValueText,        "####")]
        public void TestText(string str, TT tt, object correct)
        {
            var lexer = PrepareLexer(str);
            TestWithValue(lexer, tt, correct);
        }

        [TestMethod]
        [DataRow("\"neverendingstory", ErrorType.TextSuddenETX)]
        [DataRow("\"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAA\"", ErrorType.TextTooBig)]
        public void TestTextException(string str, ErrorType type)
        {
            var lexer = PrepareLexer(str);
            TestWithException(lexer, type);
        }

        [TestMethod]
        [DataRow("##",                  TT.ValueComment,     "")]
        [DataRow("#komentarz#",         TT.ValueComment,     "komentarz")]
        [DataRow("#koo\nmee\nntarz#",   TT.ValueComment,     "koo\nmee\nntarz")]
        [DataRow("meeeeeeeeen",         TT.ValueIdentifier,  "meeeeeeeeen")]
        [DataRow("taaaaaaarz#",         TT.ValueIdentifier,  "taaaaaaarz")]
        [DataRow("#\\#zwycięstwo\\\\#", TT.ValueComment,     "#zwycięstwo\\")]
        [DataRow("#冰淇淋#",             TT.ValueComment,     "冰淇淋")]
        [DataRow("#sadeg 😔#",          TT.ValueComment,     "sadeg 😔")]
        [DataRow("#\"\"\"\"#",          TT.ValueComment,     "\"\"\"\"")]
        [DataRow("#\\a\\b\\f\\n\\r\\t\\v\\\\\\\"\\\'#", TT.ValueComment, "\a\b\f\n\r\t\v\\\"\'")]
        public void TestComment(string str, TT tt, object correct)
        {
            var lexer = PrepareLexer(str);
            TestWithValue(lexer, tt, correct);
        }

        [TestMethod]
        [DataRow("#kooooooooo", ErrorType.CommentSuddenETX)]
        [DataRow("#AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAA#", ErrorType.CommentTooBig)]
        public void TestCommentException(string str, ErrorType type)
        {
            var lexer = PrepareLexer(str);
            TestWithException(lexer, type);
        }

        [TestMethod]
        [DataRow("FALSE",       TT.ValueFact,       false)]
        [DataRow("TRUE",        TT.ValueFact,       true)]
        [DataRow("falsetto",    TT.ValueIdentifier,  "falsetto")]
        [DataRow("abtrue",      TT.ValueIdentifier,  "abtrue")]
        [DataRow("true",        TT.ValueIdentifier,  "true")]
        [DataRow("TrUe",        TT.ValueIdentifier,  "TrUe")]
        public void TestFact(string str, TT tt, object correct)
        {
            var lexer = PrepareLexer(str);
            TestWithValue(lexer, tt, correct);
        }

        [TestMethod]
        [DataRow("count",       TT.ValueIdentifier,  "count")]
        [DataRow("_size",       TT.ValueIdentifier,  "_size")]
        [DataRow("l4id_",       TT.ValueIdentifier,  "l4id_")]
        [DataRow("13ab",        TT.ValueNumber,      13.0)]
        [DataRow("_",           TT.ValueIdentifier,  "_")]
        [DataRow("UPPERlower",  TT.ValueIdentifier,  "UPPERlower")]
        public void TestIdentifier(string str, TT tt, object correct)
        {
            var lexer = PrepareLexer(str);
            TestWithValue(lexer, tt, correct);
        }

                [TestMethod]
        [DataRow("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAA", ErrorType.IdentifierTooBig)]
        public void TestIdentifierException(string str, ErrorType type)
        {
            var lexer = PrepareLexer(str);
            TestWithException(lexer, type);
        }

        [TestMethod]
        [DataRow("\n",      TT.SpecialEOL,       null)]
        [DataRow("\r",      TT.SpecialEOL,       null)]
        [DataRow("\n\r",    TT.SpecialEOL,       null)]
        [DataRow("\r\n",    TT.SpecialEOL,       null)]
        [DataRow("\u001e",  TT.SpecialEOL,       null)]
        [DataRow("a\nb",    TT.ValueIdentifier,  "a")]
        [DataRow("\t",      TT.SpecialETX,       null)]
        public void TestNewline(string str, TT tt, object correct)
        {
            var lexer = PrepareLexer(str);
            TestWithValue(lexer, tt, correct);
        }

        [TestMethod]
        [DataRow("",        TT.SpecialETX,      null)]
        [DataRow("\uffff",  TT.SpecialETX,      null)]
        [DataRow(" ",       TT.SpecialETX,      null)]
        [DataRow("fzhjfk",  TT.ValueIdentifier, "fzhjfk")]
        public void TestETX(string str, TT tt, object correct)
        {
            var lexer = PrepareLexer(str);
            TestWithValue(lexer, tt, correct);
        }

        [TestMethod]
        [DataRow(";",       TT.OperatorDot)]
        [DataRow(",",       TT.OperatorComma)]
        [DataRow("?",       TT.OperatorTernaryYes)]
        [DataRow(":",       TT.OperatorTernaryNo)]
        [DataRow("(",       TT.OperatorParenthesisOpen)]
        [DataRow(")",       TT.OperatorParenthesisClose)]
        [DataRow("+",       TT.ArithmeticAdd)]
        [DataRow("-",       TT.ArithmeticSub)]
        [DataRow("*",       TT.ArithmeticMul)]
        [DataRow("/",       TT.ArithmeticDiv)]
        [DataRow("%",       TT.ArithmeticMod)]
        [DataRow("<",       TT.ComparatorLess)]
        [DataRow(">",       TT.ComparatorGreater)]
        [DataRow("   > ",   TT.ComparatorGreater)]
        [DataRow("==",      TT.ComparatorEqual)]
        [DataRow(">=",      TT.ComparatorGreaterEqual)]
        public void TestSingleOp(string str, TT tt)
        {
            var lexer = PrepareLexer(str);
            TestWithType(lexer, tt);
        }

        [TestMethod]
        [DataRow("@", ErrorType.UnknownToken)]
        public void TestSingleOpException(string str, ErrorType type)
        {
            var lexer = PrepareLexer(str);
            TestWithException(lexer, type);
        }

        [TestMethod]
        [DataRow("==",  TT.ComparatorEqual)]
        [DataRow("!=",  TT.ComparatorNotEqual)]
        [DataRow("<=",  TT.ComparatorLessEqual)]
        [DataRow(">=",  TT.ComparatorGreaterEqual)]
        [DataRow("++",  TT.OperatorConcatenate)]
        [DataRow("??",  TT.OperatorNoneTest)]
        [DataRow("<",   TT.ComparatorLess)]
        public void TestDoubleOp(string str, TT tt)
        {
            var lexer = PrepareLexer(str);
            TestWithType(lexer, tt);
        }

        [TestMethod]
        [DataRow("= =", ErrorType.UnknownToken)]
        [DataRow("=|", ErrorType.UnknownToken)]
        [DataRow("@@", ErrorType.UnknownToken)]
        public void TestDoubleOpException(string str, ErrorType type)
        {
            var lexer = PrepareLexer(str);
            TestWithException(lexer, type);
        }

        [TestMethod]
        [DataRow("===", TT.ComparatorEqualText)]
        [DataRow("!==", TT.ComparatorNotEqualText)]
        [DataRow("==", TT.ComparatorEqual)]
        [DataRow("!=", TT.ComparatorNotEqual)]
        public void TestTripleOp(string str, TT tt)
        {
            var lexer = PrepareLexer(str);
            TestWithType(lexer, tt);
        }

        [TestMethod]
        [DataRow("AND",         TT.KeywordAnd,          null)]
        [DataRow("BEGIN",       TT.KeywordBegin,        null)]
        [DataRow("CALL",        TT.KeywordCall,         null)]
        [DataRow("DEFAULT",     TT.KeywordDefault,      null)]
        [DataRow("DO",          TT.KeywordDo,           null)]
        [DataRow("ELSE",        TT.KeywordElse,         null)]
        [DataRow("END",         TT.KeywordEnd,          null)]
        [DataRow("FACT",        TT.KeywordFact,         null)]
        [DataRow("FOR",         TT.KeywordFor,          null)]
        [DataRow("FUNCTION",    TT.KeywordFunction,     null)]
        [DataRow("IS",          TT.KeywordIs,           null)]
        [DataRow("IF",          TT.KeywordIf,           null)]
        [DataRow("MATCH",       TT.KeywordMatch,        null)]
        [DataRow("MUTABLE",     TT.KeywordMutable,      null)]
        [DataRow("NONE",        TT.KeywordNone,         null)]
        [DataRow("NOW",         TT.KeywordNow,          null)]
        [DataRow("NOT",         TT.KeywordNot,          null)]
        [DataRow("NOTHING",     TT.KeywordNothing,      null)]
        [DataRow("NUMBER",      TT.KeywordNumber,       null)]
        [DataRow("OR",          TT.KeywordOr,           null)]
        [DataRow("OTHERWISE",   TT.KeywordOtherwise,    null)]
        [DataRow("OVERRIDE",    TT.KeywordOverride,     null)]
        [DataRow("PATTERN",     TT.KeywordPattern,      null)]
        [DataRow("RETURN",      TT.KeywordReturn,       null)]
        [DataRow("RETURNS",     TT.KeywordReturns,      null)]
        [DataRow("SKIP",        TT.KeywordSkip,         null)]
        [DataRow("STOP",        TT.KeywordStop,         null)]
        [DataRow("TEXT",        TT.KeywordText,         null)]
        [DataRow("THEN",        TT.KeywordThen,         null)]
        [DataRow("WHILE",       TT.KeywordWhile,        null)]
        [DataRow("WITH",        TT.KeywordWith,         null)]
        [DataRow("and",         TT.ValueIdentifier,     "and")]
        [DataRow("aND",         TT.ValueIdentifier,     "aND")]
        [DataRow("ANDY",        TT.ValueIdentifier,     "ANDY")]
        [DataRow("BAND",        TT.ValueIdentifier,     "BAND")]
        [DataRow("PIPE",        TT.ValueIdentifier,     "PIPE")]
        [DataRow("VALUE",       TT.ValueIdentifier,     "VALUE")]
        public void TestKeyword(string str, TT tt, object correct)
        {
            var lexer = PrepareLexer(str);
            TestWithValue(lexer, tt, correct);
        }
    }
}
