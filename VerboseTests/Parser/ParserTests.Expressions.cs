using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Interfaces;
using VerboseCore.Helpers;
using VerboseCore.Parser;

using static VerboseTests.Parser_.ParserTests;

using UOT = VerboseCore.Entities.UnaryOperatorType;
using BOT = VerboseCore.Entities.BinaryOperatorType;

namespace VerboseTests.Parser_
{
    [TestClass]
    public class ExpressionTests
    {

        // operator_1 = CALL, IDENTIFIER, [WITH, arguments], NOW;
        // arguments  = expression, {COMMA, expression
        [TestMethod]
        public void TestCallExpressionSimple()
        {
            List<IToken> tokenList = new()
            {
                // CALL a NOW
                KeywordT("CALL"), IdentifierT("a"), KeywordT("NOW"),
            };
            IInstruction expected = new ExpressionCall(IdentifierE("a"), new() { });

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildCallExpression);
        }

        // operator_1 = CALL, IDENTIFIER, [WITH, arguments], NOW;
        // arguments  = expression, {COMMA, expression
        [TestMethod]
        public void TestCallExpressionParam()
        {
            List<IToken> tokenList = new()
            {
                // CALL a WITH 1 NOW
                KeywordT("CALL"),
                IdentifierT("a"),
                KeywordT("WITH"),
                LiteralT(1),
                KeywordT("NOW"),
            };
            IInstruction expected = new ExpressionCall(IdentifierE("a"), new() { LiteralE(1) });

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildCallExpression);
        }

        // operator_1 = CALL, IDENTIFIER, [WITH, arguments], NOW;
        // arguments  = expression, {COMMA, expression
        [TestMethod]
        public void TestCallExpressionMultipleParam()
        {
            List<IToken> tokenList = new()
            {
                // CALL a WITH 1, 2, 3 NOW
                KeywordT("CALL"),
                IdentifierT("a"),
                KeywordT("WITH"),
                LiteralT(1),
                OperatorT(","),
                LiteralT(2),
                OperatorT(","),
                LiteralT(3),
                KeywordT("NOW"),
            };
            IInstruction expected = new ExpressionCall(IdentifierE("a"), 
                new() { LiteralE(1), LiteralE(2), LiteralE(3) });

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildCallExpression);
        }

        // operator_1 = CALL, IDENTIFIER, [WITH, arguments], NOW;
        // arguments  = expression, {COMMA, expression
        [TestMethod]
        public void TestCallExpressionNested()
        {
            List<IToken> tokenList = new()
            {
                // CALL a WITH {CALL b NOW} NOW
                KeywordT("CALL"),
                IdentifierT("a"),
                KeywordT("WITH"),
                KeywordT("CALL"),
                IdentifierT("b"),
                KeywordT("NOW"),
                KeywordT("NOW"),
            };
            IInstruction expected = new ExpressionCall(IdentifierE("a"),
                new() { new ExpressionCall(IdentifierE("b"), new() { }) });

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildCallExpression);
        }

        // operator_2 = (LITERAL | IDENTIFIER | PIPE | VALUE | operator_1 | (PARENTHESIS_OPEN, expression, PARENTHESIS_CLOSE)), [TEST_NONE];
        [TestMethod]
        public void TestNoneTestExpressionSimple()
        {
            List<List<IToken>> tokenLists = new()
            {
                // 1
                new() { LiteralT(1) },
                // a
                new() { IdentifierT("a") },
                // PIPE
                new() { IdentifierT("PIPE") },
                // VALUE
                new() { IdentifierT("VALUE") },
                // 1 ??
                new() { LiteralT(1), OperatorT("??")}
            };
            List<IInstruction?> expected = new()
            {
                LiteralE(1),
                IdentifierE("a"),
                IdentifierE("PIPE"),
                IdentifierE("VALUE"),
                new ExpressionNoneTest(LiteralE(1)),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildNoneTestExpression);
        }

        // operator_2 = (LITERAL | IDENTIFIER | PIPE | VALUE | operator_1 | (PARENTHESIS_OPEN, expression, PARENTHESIS_CLOSE)), [TEST_NONE];
        [TestMethod]
        public void TestNoneTestExpressionParenthesis()
        {
            List<IToken> tokenList = new()
            {
                // (1)
                OperatorT("("), LiteralT(1), OperatorT(")")
            };
            IInstruction? expected = LiteralE(1);

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildNoneTestExpression);
        }

        // operator_3 = [MINUS | NOT], operator_2;
        [TestMethod]
        public void TestUnaryExpressionSimple()
        {
            List<List<IToken>> tokenLists = new()
            {
                // -1
                new() { OperatorT("-"), LiteralT(1) },
                // NOT a
                new() { KeywordT("NOT"), IdentifierT("a") },
            };
            List<IInstruction?> expected = new()
            {
                new ExpressionUnary(LiteralE(1), UOT.ArithmeticNegate),
                new ExpressionUnary(IdentifierE("a"), UOT.LogicalNot),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildUnaryExpression);
        }

        // operator_4 = operator_3, {(MULTIPLY | DIVIDE | MODULO), operator_3};
        [TestMethod]
        public void TestBinaryExpressionMultiply()
        {
            List<List<IToken>> tokenLists = new()
            {
                // 1  *  2
                new() { LiteralT(1), OperatorT("*"), LiteralT(2) },
                // 1  /  2
                new() { LiteralT(1), OperatorT("/"), LiteralT(2) },
                // 1  %  2
                new() { LiteralT(1), OperatorT("%"), LiteralT(2) },
                // 1  *  2 * 3
                new() { LiteralT(1), OperatorT("*"), LiteralT(2), OperatorT("*"), LiteralT(3) },
            };
            List<IInstruction?> expected = new()
            {
                BinaryE(LiteralE(1), BOT.ArithmeticMul, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.ArithmeticDiv, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.ArithmeticMod, LiteralE(2)),
                BinaryE(BinaryE(LiteralE(1), BOT.ArithmeticMul, LiteralE(2)), BOT.ArithmeticMul, LiteralE(3)),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildBinaryExpressionMultiplication);
        }

        // operator_5 = operator_4, {(PLUS | MINUS | CONCAT), operator_4};
        [TestMethod]
        public void TestBinaryExpressionAdd()
        {
            List<List<IToken>> tokenLists = new()
            {
                // 1  +  2
                new() { LiteralT(1), OperatorT("+"), LiteralT(2) },
                // 1  -  2
                new() { LiteralT(1), OperatorT("-"), LiteralT(2) },
                // 1 ++  2
                new() { LiteralT(1), OperatorT("++"), LiteralT(2) },
            };
            List<IInstruction?> expected = new()
            {
                BinaryE(LiteralE(1), BOT.ArithmeticAdd, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.ArithmeticSub, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.OperatorConcatenate, LiteralE(2)),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildBinaryExpressionAddition);
        }

        // operator_6 = operator_5, {(GREATER | GREATER_EQUAL | LESSER | LESSER_EQUAL), operator_5};
        [TestMethod]
        public void TestBinaryExpressionGreater()
        {
            List<List<IToken>> tokenLists = new()
            {
                // 1  >  2
                new() { LiteralT(1), OperatorT(">"), LiteralT(2) },
                // 1 >=  2
                new() { LiteralT(1), OperatorT(">="), LiteralT(2) },
                // 1  <  2
                new() { LiteralT(1), OperatorT("<"), LiteralT(2) },
                // 1 <=  2
                new() { LiteralT(1), OperatorT("<="), LiteralT(2) },
            };
            List<IInstruction?> expected = new()
            {
                BinaryE(LiteralE(1), BOT.ComparatorGreater, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.ComparatorGreaterEqual, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.ComparatorLess, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.ComparatorLessEqual, LiteralE(2)),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildBinaryExpressionGreater);
        }

        // operator_7 = operator_6, {(EQUAL | NOT_EQUAL | TEXT_EQUAL | TEXT_NOT_EQUAL), operator_6};
        [TestMethod]
        public void TestBinaryExpressionEqual()
        {
            List<List<IToken>> tokenLists = new()
            {
                // 1 ==  2
                TrailDot(new() { LiteralT(1), OperatorT("=="), LiteralT(2) }),
                // 1 !=  2
                TrailDot(new() { LiteralT(1), OperatorT("!="), LiteralT(2) }),
                // 1 ===  2
                TrailDot(new() { LiteralT(1), OperatorT("==="), LiteralT(2) }),
                // 1 !==  2
                TrailDot(new() { LiteralT(1), OperatorT("!=="), LiteralT(2) }),
            };
            List<IInstruction?> expected = new()
            {
                BinaryE(LiteralE(1), BOT.ComparatorEqual, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.ComparatorNotEqual, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.ComparatorEqualText, LiteralE(2)),
                BinaryE(LiteralE(1), BOT.ComparatorNotEqualText, LiteralE(2)),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildBinaryExpressionEqual);
        }

        // operator_8 = operator_7, {AND, operator_7};
        [TestMethod]
        public void TestBinaryExpressionAnd()
        {
            List<List<IToken>> tokenLists = new()
            {
                // 1 AND 2
                TrailDot(new() { LiteralT(1), KeywordT("AND"), LiteralT(2) }),
            };
            List<IInstruction?> expected = new()
            {
                BinaryE(LiteralE(1), BOT.LogicalAnd, LiteralE(2)),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildBinaryExpressionAnd);
        }

        // operator_9 = operator_8, {OR, operator_8};
        [TestMethod]
        public void TestBinaryExpressionOr()
        {
            List<List<IToken>> tokenLists = new()
            {
                // 1 OR  2
                TrailDot(new() { LiteralT(1), KeywordT("OR"), LiteralT(2) }),
            };
            List<IInstruction?> expected = new()
            {
                BinaryE(LiteralE(1), BOT.LogicalOr, LiteralE(2)),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildBinaryExpressionOr);
        }

        //
        [TestMethod]
        public void TestBinaryExpressionPriority()
        {
            List<List<IToken>> tokenLists = new()
            {
                // {1 * 2} + 3
                TrailDot(new() { LiteralT(1), OperatorT("*"), LiteralT(2), OperatorT("+"), LiteralT(3) }),
                // 1 * (2 + 3)
                TrailDot(new()
                {
                    LiteralT(1),
                    OperatorT("*"),
                    OperatorT("("),
                    LiteralT(2),
                    OperatorT("+"),
                    LiteralT(3),
                    OperatorT(")")
                }),
            };
            List<IInstruction?> expected = new()
            {
                BinaryE(
                    BinaryE(LiteralE(1), BOT.ArithmeticMul, LiteralE(2)),
                    BOT.ArithmeticAdd,
                    LiteralE(3)
                ),
                BinaryE(
                    LiteralE(1),
                    BOT.ArithmeticMul,
                    BinaryE(LiteralE(2), BOT.ArithmeticAdd, LiteralE(3))
                ),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildBinaryExpressionOr);
        }

        // operator_10 = operator_9, {TERNARY_YES, operator_9, [TERNARY_NO, operator_9]};
        [TestMethod]
        public void TestTernaryExpressionSimple()
        {
            List<IToken> tokenList = new()
            {
                // 1 ? 2 : 3
                LiteralT(1), OperatorT("?"), LiteralT(2), OperatorT(":"), LiteralT(3)
            };
            IInstruction? expected = new ExpressionTernary(LiteralE(1), LiteralE(2), LiteralE(3));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildTernaryExpression);
        }

        // operator_10 = operator_9, {TERNARY_YES, operator_9, [TERNARY_NO, operator_9]};
        [TestMethod]
        public void TestTernaryExpressionWithoutElse()
        {
            List<IToken> tokenList = new()
            {
                // 1 ? 2
                LiteralT(1),
                OperatorT("?"),
                LiteralT(2),
            };
            IInstruction? expected = new ExpressionTernary(LiteralE(1), LiteralE(2), null);

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildTernaryExpression);
        }

        // operator_10 = operator_9, {TERNARY_YES, operator_9, [TERNARY_NO, operator_9]};
        [TestMethod]
        public void TestTernaryExpressionNested()
        {
            List<IToken> tokenList = new()
            {
                // {1 ? 2} ? 3 : 4
                LiteralT(1),
                OperatorT("?"),
                LiteralT(2),
                OperatorT("?"),
                LiteralT(3),
                OperatorT(":"),
                LiteralT(4)
            };
            IInstruction? expected = new ExpressionTernary(
                new ExpressionTernary(LiteralE(1), LiteralE(2), null),
                LiteralE(3),
                LiteralE(4));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildTernaryExpression);
        }

        // operator_11 = operator_10, {[THEN, operator_10], [OTHERWISE, operator_10]};
        [TestMethod]
        public void TestPipeExpressionSimple()
        {
            List<IToken> tokenList = new()
            {
                // 1 THEN 2 OTHERWISE 3
                LiteralT(1),
                KeywordT("THEN"),
                LiteralT(2),
                KeywordT("OTHERWISE"),
                LiteralT(3)
            };
            IInstruction? expected = new ExpressionPipe(LiteralE(1), LiteralE(2), LiteralE(3));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildPipeExpression);
        }

        // operator_11 = operator_10, {[THEN, operator_10], [OTHERWISE, operator_10]};
        [TestMethod]
        public void TestPipeExpressionWithoutThen()
        {
            List<IToken> tokenList = new()
            {
                // 1 OTHERWISE 2
                LiteralT(1), KeywordT("OTHERWISE"), LiteralT(2)
            };
            IInstruction? expected = new ExpressionPipe(LiteralE(1), null, LiteralE(2));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildPipeExpression);
        }

        // operator_11 = operator_10, {[THEN, operator_10], [OTHERWISE, operator_10]};
        [TestMethod]
        public void TestPipeExpressionWithoutOtherwise()
        {
            List<IToken> tokenList = new()
            {
                // 1 THEN 2
                LiteralT(1), KeywordT("THEN"), LiteralT(2)
            };
            IInstruction? expected = new ExpressionPipe(LiteralE(1), LiteralE(2), null);

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildPipeExpression);
        }

        // operator_11 = operator_10, {[THEN, operator_10], [OTHERWISE, operator_10]};
        [TestMethod]
        public void TestPipeExpressionNested()
        {
            List<IToken> tokenList = new()
            {
                // {{1 THEN 2} THEN 3 OTHERWISE 4} OTHERWISE 5
                LiteralT(1),
                KeywordT("THEN"),
                LiteralT(2),
                KeywordT("THEN"),
                LiteralT(3),
                KeywordT("OTHERWISE"),
                LiteralT(4),
                KeywordT("OTHERWISE"),
                LiteralT(5)
            };
            IInstruction? expected = new ExpressionPipe(
                new ExpressionPipe(
                    new ExpressionPipe(LiteralE(1), LiteralE(2), null),
                    LiteralE(3),
                    LiteralE(4)
                ),
                null,
                LiteralE(5)
            );

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildPipeExpression);
        }

        // operator_12 = {operator_11, IS}, operator_11;
        [TestMethod]
        public void TestAssignmentExpressionSimple()
        {
            List<IToken> tokenList = new()
            {
                // a IS 1
                IdentifierT("a"), KeywordT("IS"), LiteralT(1)
            };
            IInstruction? expected = AssignmentE(LiteralE(1), IdentifierE("a"));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildAssignmentExpression);
        }

        // operator_12 = {operator_11, IS}, operator_11;
        [TestMethod]
        public void TestAssignmentExpressionMultiple()
        {
            List<IToken> tokenList = new()
            {
                // a IS {b IS {c IS 2}};
                IdentifierT("a"),
                KeywordT("IS"),
                IdentifierT("b"),
                KeywordT("IS"),
                IdentifierT("c"),
                KeywordT("IS"),
                LiteralT(2),
            };
            IInstruction? expected = AssignmentE(
                AssignmentE(
                    AssignmentE(
                        LiteralE(2),
                        IdentifierE("c")
                        ),
                    IdentifierE("b")
                ),
                IdentifierE("a")
            );

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildAssignmentExpression);
        }
    }
}
