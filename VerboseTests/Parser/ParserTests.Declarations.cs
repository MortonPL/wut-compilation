using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Helpers;
using VerboseCore.Interfaces;
using VerboseCore.Parser;

using static VerboseTests.Parser_.ParserTests;

using BOT = VerboseCore.Entities.BinaryOperatorType;
using VT = VerboseCore.Entities.VariableType;
using JT = VerboseCore.Entities.JumpType;

namespace VerboseTests.Parser_
{
    [TestClass]
    public class DeclarationTests
    {
        // variable_declaration = declarator, [IS, expression];
        // declarator = [MUTABLE], type, IDENTIFIER;
        [TestMethod]
        public void TestVariableDeclarationSimple()
        {
            List<IToken> tokenList = new()
            {
                // TEXT a
                KeywordT("TEXT"),
                IdentifierT("a"),
            };
            IInstruction? expected = VariableD(false, VT.Text, IdentifierE("a"), null);

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildVariableDeclaration);
        }

        // variable_declaration = declarator, [IS, expression];
        // declarator = [MUTABLE], type, IDENTIFIER;
        [TestMethod]
        public void TestVariableDeclarationMutable()
        {
            List<IToken> tokenList = new()
            {
                // MUTABLE TEXT a
                KeywordT("MUTABLE"),
                KeywordT("TEXT"),
                IdentifierT("a"),
            };
            IInstruction? expected = VariableD(true, VT.Text, IdentifierE("a"), null);

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildVariableDeclaration);
        }

        // variable_declaration = declarator, [IS, expression];
        // declarator = [MUTABLE], type, IDENTIFIER;
        [TestMethod]
        public void TestVariableDeclarationInitialization()
        {
            List<IToken> tokenList = new()
            {
                // MUTABLE TEXT a IS 1
                KeywordT("MUTABLE"),
                KeywordT("TEXT"),
                IdentifierT("a"),
                KeywordT("IS"),
                LiteralT(1),
            };
            IInstruction? expected = VariableD(true, VT.Text, IdentifierE("a"), LiteralE(1));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildVariableDeclaration);
        }

        // function_declaration = FUNCTION, [OVERRIDE], IDENTIFIER, [WITH, parameters], RETURNS, return_type, statement;
        // parameters = declarator, {COMMA, declarator}
        // declarator = [MUTABLE], type, IDENTIFIER;
        [TestMethod]
        public void TestFunctionDeclarationSimple()
        {
            List<IToken> tokenList = new()
            {
                // FUNCTION a RETURNS NOTHING b;
                KeywordT("FUNCTION"),
                IdentifierT("a"),
                KeywordT("RETURNS"),
                KeywordT("NOTHING"),
                IdentifierT("b"),
                DotT(),
            };
            IInstruction? expected = FunctionD(VT.Nothing, IdentifierE("a"),
                new List<IDeclarator>() { }, IdentifierE("b"));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildFunctionDeclaration);
        }

        // function_declaration = FUNCTION, [OVERRIDE], IDENTIFIER, [WITH, parameters], RETURNS, return_type, statement;
        // parameters = declarator, {COMMA, declarator}
        // declarator = [MUTABLE], type, IDENTIFIER;
        [TestMethod]
        public void TestFunctionDeclarationOverride()
        {
            List<IToken> tokenList = new()
            {
                // FUNCTION OVERRIDE a RETURNS NOTHING b;
                KeywordT("FUNCTION"),
                KeywordT("OVERRIDE"),
                IdentifierT("a"),
                KeywordT("RETURNS"),
                KeywordT("NOTHING"),
                IdentifierT("b"),
                DotT(),
            };
            IInstruction? expected = FunctionD(VT.Nothing, IdentifierE("a"),
                new List<IDeclarator>() { }, IdentifierE("b"), true);

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildFunctionDeclaration);
        }

        // function_declaration = FUNCTION, [OVERRIDE], IDENTIFIER, [WITH, parameters], RETURNS, return_type, statement;
        // parameters = declarator, {COMMA, declarator}
        // declarator = [MUTABLE], type, IDENTIFIER;
        [TestMethod]
        public void TestFunctionDeclarationParameter()
        {
            List<IToken> tokenList = new()
            {
                // FUNCTION a WITH TEXT b RETURNS NOTHING c;
                KeywordT("FUNCTION"),
                IdentifierT("a"),
                KeywordT("WITH"),
                KeywordT("TEXT"),
                IdentifierT("b"),
                KeywordT("RETURNS"),
                KeywordT("NOTHING"),
                IdentifierT("c"),
                DotT(),
            };
            IInstruction? expected = FunctionD(VT.Nothing, IdentifierE("a"),
                new List<IDeclarator>() { Declarator(false, VT.Text, IdentifierE("b")) }, IdentifierE("c"));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildFunctionDeclaration);
        }

        // function_declaration = FUNCTION, [OVERRIDE], IDENTIFIER, [WITH, parameters], RETURNS, return_type, statement;
        // parameters = declarator, {COMMA, declarator}
        // declarator = [MUTABLE], type, IDENTIFIER;
        [TestMethod]
        public void TestFunctionDeclarationMultipleParameters()
        {
            List<IToken> tokenList = new()
            {
                // FUNCTION a WITH TEXT b, FACT c, NUMBER d RETURNS NUMBER e;
                KeywordT("FUNCTION"),
                IdentifierT("a"),
                KeywordT("WITH"),
                KeywordT("TEXT"),
                IdentifierT("b"),
                OperatorT(","),
                KeywordT("FACT"),
                IdentifierT("c"),
                OperatorT(","),
                KeywordT("NUMBER"),
                IdentifierT("d"),
                KeywordT("RETURNS"),
                KeywordT("NUMBER"),
                IdentifierT("e"),
                DotT(),
            };
            IInstruction? expected = FunctionD(VT.Number, IdentifierE("a"),
                new List<IDeclarator>()
                {
                    Declarator(false, VT.Text, IdentifierE("b")),
                    Declarator(false, VT.Fact, IdentifierE("c")),
                    Declarator(false, VT.Number, IdentifierE("d"))
                }, IdentifierE("e"));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildFunctionDeclaration);
        }

        // function_declaration = FUNCTION, [OVERRIDE], IDENTIFIER, [WITH, parameters], RETURNS, return_type, statement;
        // parameters = declarator, {COMMA, declarator}
        // declarator = [MUTABLE], type, IDENTIFIER;
        [TestMethod]
        public void TestFunctionDeclarationBigBody()
        {
            List<IToken> tokenList = new()
            {
                // FUNCTION a RETURNS FACT BEGIN NUMBER b IS 1+2; RETURN b > 0; END
                KeywordT("FUNCTION"),
                IdentifierT("a"),
                KeywordT("RETURNS"),
                KeywordT("FACT"),
                KeywordT("BEGIN"),
                KeywordT("NUMBER"),
                IdentifierT("b"),
                KeywordT("IS"),
                LiteralT(1),
                OperatorT("+"),
                LiteralT(2),
                DotT(),
                KeywordT("RETURN"),
                IdentifierT("b"),
                OperatorT(">"),
                LiteralT(0),
                DotT(),
                KeywordT("END")
            };
            IInstruction? expected = FunctionD(VT.Fact, IdentifierE("a"), new List<IDeclarator>() { },
                CompoundS(new()
                {
                    VariableD(false, VT.Number, IdentifierE("b"), BinaryE(LiteralE(1), BOT.ArithmeticAdd, LiteralE(2))),
                    new StatementJump(JT.Return, BinaryE(IdentifierE("b"), BOT.ComparatorGreater, LiteralE(0))),
                })
            );

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildFunctionDeclaration);
        }

        // pattern_declaration = PATTERN, [OVERRIDE], IDENTIFIER, WITH, declarator, (match_block | DOT);
        // match_block = BEGIN, {match_branch}, DEFAULT, statement, COMMA, END;
        // match_branch = expression, DO, statement, COMMA;
        // note: tested less because match_block is already covered in anon match tests
        [TestMethod]
        public void TestPatternDeclarationSingleBranch()
        {
            List<IToken> tokenList = new()
            {
                // PATTERN a WITH FACT b BEGIN
                // VALUE DO 1;,
                // DEFAULT 2;,
                // END
                KeywordT("PATTERN"),
                IdentifierT("a"),
                KeywordT("WITH"),
                KeywordT("FACT"),
                IdentifierT("b"),
                KeywordT("BEGIN"),
                IdentifierT("VALUE"),
                KeywordT("DO"),
                LiteralT(1),
                DotT(),
                OperatorT(","),
                KeywordT("DEFAULT"),
                LiteralT(2),
                DotT(),
                OperatorT(","),
                KeywordT("END")
            };
            IInstruction? expected = PatternD(IdentifierE("a"), Declarator(false, VT.Fact, IdentifierE("b")), CompoundS(new()
            {
                IfS(IdentifierE("VALUE"), LiteralE(1), null),
                IfS(LiteralE(true), LiteralE(2), null),
            }));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildPatternDeclaration);
        }

        // pattern_declaration = PATTERN, [OVERRIDE], IDENTIFIER, WITH, declarator, (match_block | DOT);
        // match_block = BEGIN, {match_branch}, DEFAULT, statement, COMMA, END;
        // match_branch = expression, DO, statement, COMMA;
        [TestMethod]
        public void TestPatternDeclarationNoBody()
        {
            List<IToken> tokenList = new()
            {
                // PATTERN OVERRIDE a WITH FACT b;
                KeywordT("PATTERN"),
                KeywordT("OVERRIDE"),
                IdentifierT("a"),
                KeywordT("WITH"),
                KeywordT("FACT"),
                IdentifierT("b"),
                DotT(),
            };
            IInstruction? expected = PatternD(IdentifierE("a"), Declarator(false, VT.Fact, IdentifierE("b")), new StatementEmpty(), true);

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildPatternDeclaration);
        }

        // pattern_declaration = PATTERN, [OVERRIDE], IDENTIFIER, WITH, declarator, (match_block | DOT);
        // match_block = BEGIN, {match_branch}, DEFAULT, statement, COMMA, END;
        // match_branch = expression, DO, statement, COMMA;
        [TestMethod]
        public void TestPatternDeclarationOverride()
        {
            List<IToken> tokenList = new()
            {
                // PATTERN OVERRIDE a WITH FACT b BEGIN
                // VALUE DO 1;,
                // DEFAULT 2;,
                // END
                KeywordT("PATTERN"),
                KeywordT("OVERRIDE"),
                IdentifierT("a"),
                KeywordT("WITH"),
                KeywordT("FACT"),
                IdentifierT("b"),
                KeywordT("BEGIN"),
                IdentifierT("VALUE"),
                KeywordT("DO"),
                LiteralT(1),
                DotT(),
                OperatorT(","),
                KeywordT("DEFAULT"),
                LiteralT(2),
                DotT(),
                OperatorT(","),
                KeywordT("END")
            };
            IInstruction? expected = PatternD(IdentifierE("a"), Declarator(false, VT.Fact, IdentifierE("b")), CompoundS(new()
            {
                IfS(IdentifierE("VALUE"), LiteralE(1), null),
                IfS(LiteralE(true), LiteralE(2), null),
            }), true);

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildPatternDeclaration);
        }
    }
}
