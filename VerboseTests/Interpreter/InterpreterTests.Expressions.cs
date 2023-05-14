using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Helpers;
using VerboseCore.Interfaces;

using VerboseCLI.Interpreter;

using static VerboseTests.Parser_.ParserTests;
using static VerboseTests.Interpreter_.InterpreterTests;
using VT = VerboseCore.Entities.VariableType;
using UOT = VerboseCore.Entities.UnaryOperatorType;
using BOT = VerboseCore.Entities.BinaryOperatorType;

namespace VerboseTests.Interpreter_
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public void TestExpressionLiteral()
        {
            List<IExpressionLiteral> programs = new()
            {
                LiteralE(1),
                LiteralE("aaaBBB"),
                LiteralE(true),
                LiteralE(0.155),
            };

            List<object?> expected = new()
            {
                1,
                "aaaBBB",
                true,
                0.155,
            };

            var interpreter = new Interpreter(new MockParser(), new MockLogger());
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionBinaryArithmetic()
        {
            List<IExpressionBinary> programs = new()
            {
                // 2 + 2
                BinaryE(LiteralE(2), BOT.ArithmeticAdd, LiteralE(2)),
                // 2 - 2
                BinaryE(LiteralE(2), BOT.ArithmeticSub, LiteralE(2)),
                // 2 * 2
                BinaryE(LiteralE(2), BOT.ArithmeticMul, LiteralE(2)),
                // 2 / 2
                BinaryE(LiteralE(2), BOT.ArithmeticDiv, LiteralE(2)),
                // 2 % 2
                BinaryE(LiteralE(2), BOT.ArithmeticMod, LiteralE(2)),
                // "2" + TRUE
                BinaryE(LiteralE("2"), BOT.ArithmeticAdd, LiteralE(true)),
                // 2 + 2 + 2
                BinaryE(BinaryE(LiteralE(2), BOT.ArithmeticAdd, LiteralE(2)), BOT.ArithmeticAdd, LiteralE(2)),
            };

            List<object?> expected = new()
            {
                4, // 2 + 2
                0, // 2 - 2
                4, // 2 * 2
                1, // 2 / 2
                0, // 2 % 2
                3, // "2" + TRUE
                6, // 2 + 2 + 2
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionBinaryLogical()
        {
            List<IExpressionBinary> programs = new()
            {
                // TRUE AND TRUE
                BinaryE(LiteralE(true), BOT.LogicalAnd, LiteralE(true)),
                // FALSE OR TRUE
                BinaryE(LiteralE(true), BOT.LogicalOr, LiteralE(false)),
                // FALSE AND TRUE OR FALSE
                BinaryE(BinaryE(LiteralE(false), BOT.LogicalAnd, LiteralE(true)), BOT.LogicalOr, LiteralE(false)),
            };

            List<object?> expected = new()
            {
                true,
                true,
                false,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionComparisonNumber()
        {
            List<IExpressionBinary> programs = new()
            {
                // 1 == 1
                BinaryE(LiteralE(1), BOT.ComparatorEqual, LiteralE(1)),
                // 1 == 2
                BinaryE(LiteralE(1), BOT.ComparatorEqual, LiteralE(2)),
                // 1 != 1
                BinaryE(LiteralE(1), BOT.ComparatorNotEqual, LiteralE(1)),
                // 1 != 2
                BinaryE(LiteralE(1), BOT.ComparatorNotEqual, LiteralE(2)),
                // 1 > 2
                BinaryE(LiteralE(1), BOT.ComparatorGreater, LiteralE(2)),
                // 1 >= 2
                BinaryE(LiteralE(1), BOT.ComparatorGreaterEqual, LiteralE(2)),
                // 1 < 2
                BinaryE(LiteralE(1), BOT.ComparatorLess, LiteralE(2)),
                // 1 <= 2
                BinaryE(LiteralE(1), BOT.ComparatorLessEqual, LiteralE(2)),
                // 1 == TRUE
                BinaryE(LiteralE(1), BOT.ComparatorEqual, LiteralE(true)),
                // 2 >= "1"
                BinaryE(LiteralE(2), BOT.ComparatorGreaterEqual, LiteralE("1")),
                // 1 != 2 != 3       // TRUE != 3 // TRUE != TRUE // FALSE
                BinaryE(BinaryE(LiteralE(1), BOT.ComparatorNotEqual, LiteralE(2)), BOT.ComparatorNotEqual, LiteralE(3)),
                // 1 != (2 != 3)     // 1 != TRUE // TRUE != TRUE // FALSE
                BinaryE(LiteralE(1), BOT.ComparatorNotEqual, BinaryE(LiteralE(2), BOT.ComparatorNotEqual, LiteralE(3))),
            };

            List<object?> expected = new()
            {
                true,  // 1 == 1
                false, // 1 == 2;
                false, // 1 != 
                true,  // 1 != 2
                false, // 1 > 2
                false, // 1 >= 2
                true,  // 1 < 2
                true,  // 1 <= 2
                true,  // 1 == TRUE
                true,  // 2 >= "1"
                false, // 1 != 2 != 3
                false  // 1 != (2 != 3)
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionDivideByZero()
        {
            IExpressionBinary program =
                // 1 / 0
                BinaryE(LiteralE(1), BOT.ArithmeticDiv, LiteralE(0));

            ErrorType expected = ErrorType.DivisionByZero;

            var interpreter = MakeInterpreter();
            TestInterpreterException(program, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionComparisonText()
        {
            List<IExpressionBinary> programs = new()
            {
                // "a" == "b"
                BinaryE(LiteralE("a"), BOT.ComparatorEqual, LiteralE("b")),
                // "a" == "bb"
                BinaryE(LiteralE("a"), BOT.ComparatorEqual, LiteralE("bb")),
                // "a" != "b"
                BinaryE(LiteralE("a"), BOT.ComparatorNotEqual, LiteralE("b")),
                // "a" != "bb"
                BinaryE(LiteralE("a"), BOT.ComparatorNotEqual, LiteralE("bb")),
                // "aa" > "b"
                BinaryE(LiteralE("aa"), BOT.ComparatorGreater, LiteralE("b")),
                // "" >= "b"
                BinaryE(LiteralE(""), BOT.ComparatorGreaterEqual, LiteralE("b")),
                // "a" < "bb"
                BinaryE(LiteralE("a"), BOT.ComparatorLess, LiteralE("bb")),
                // "" <= ""
                BinaryE(LiteralE(""), BOT.ComparatorLessEqual, LiteralE("")),
                // "aaaa" == TRUE
                BinaryE(LiteralE("aaaa"), BOT.ComparatorEqual, LiteralE(true)),
                // "bb" >= 0
                BinaryE(LiteralE("bb"), BOT.ComparatorGreaterEqual, LiteralE(0)),
                // "aaa" === "bbb"
                BinaryE(LiteralE("aaa"), BOT.ComparatorEqualText, LiteralE("bbb")),
                // "aa" !== "bb"
                BinaryE(LiteralE("aa"), BOT.ComparatorNotEqualText, LiteralE("bb")),
                // "TRUE" === TRUE
                BinaryE(LiteralE("TRUE"), BOT.ComparatorEqualText, LiteralE(true)),
                // 1 === 1
                BinaryE(LiteralE(1), BOT.ComparatorEqualText, LiteralE(1)),
            };

            List<object?> expected = new()
            {
                true,  // "a" == "b"
                false, // "a" == "bb"
                false, // "a" != "b"
                true,  // "a" != "bb"
                true,  // "aa" > "b"
                false, // "" >= "b"
                true,  // "a" < "bb"
                true,  // "" <= ""
                true,  // "aaaa" == TRUE
                true,  // "bb" >= 0
                false, // "aaa" === "bbb"
                true,  // "aa" !== "bb"
                true,  // "TRUE" === TRUE
                true,  // 1 === 1
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionComparisonFact()
        {
            List<IExpressionBinary> programs = new()
            {
                // TRUE == TRUE     // TRUE AND TRUE
                BinaryE(LiteralE(true), BOT.ComparatorEqual, LiteralE(true)),
                // TRUE == FALSE    // TRUE AND FALSE
                BinaryE(LiteralE(true), BOT.ComparatorEqual, LiteralE(false)),
                // TRUE != TRUE     // TRUE XOR TRUE
                BinaryE(LiteralE(true), BOT.ComparatorNotEqual, LiteralE(true)),
                // TRUE != FALSE    // TRUE XOR FALSE
                BinaryE(LiteralE(true), BOT.ComparatorNotEqual, LiteralE(false)),
                // TRUE > FALSE     // TRUE AND NOT FALSE
                BinaryE(LiteralE(true), BOT.ComparatorGreater, LiteralE(false)),
                // TRUE >= TRUE     // fałszywe tylko dla FALSE >= TRUE
                BinaryE(LiteralE(true), BOT.ComparatorGreaterEqual, LiteralE(true)),
                // FALSE < FALSE    //  NOT FALSE AND FALSE
                BinaryE(LiteralE(false), BOT.ComparatorLess, LiteralE(false)),
                // FALSE <= TRUE    // fałszywe tylko dla TRUE <= FALSE, implikacja
                BinaryE(LiteralE(false), BOT.ComparatorLessEqual, LiteralE(true)),
                // FALSE == 0
                BinaryE(LiteralE(false), BOT.ComparatorEqual, LiteralE(0)),
                // TRUE == "a"
                BinaryE(LiteralE(true), BOT.ComparatorEqual, LiteralE("a")),
            };

            List<object?> expected = new()
            {
                true,  // TRUE == TRUE
                false, // TRUE == FALSE
                false, // TRUE != TRUE
                true,  // TRUE != FALSE
                true,  // TRUE > FALSE
                true,  // TRUE >= TRUE
                false, // FALSE < FALSE
                true,  // FALSE <= TRUE
                true,  // FALSE == 0
                true,  // TRUE == "a"
                false  // TRUE == ""
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionConcatenation()
        {
            List<IExpressionBinary> programs = new()
            {
                // "a" + "b"    // NONE + NONE
                BinaryE(LiteralE("a"), BOT.ArithmeticAdd, LiteralE("b")),
                // "a" ++ "b"
                BinaryE(LiteralE("a"), BOT.OperatorConcatenate, LiteralE("b")),
                // "a" ++ "b" ++ "c"
                BinaryE(BinaryE(LiteralE("a"), BOT.OperatorConcatenate, LiteralE("b")), BOT.OperatorConcatenate, LiteralE("c")),
            };

            List<object?> expected = new()
            {
                null,  // "a" + "b"
                "ab",  // "a" ++ "b"
                "abc"  // "a" ++ "b" ++ "c"
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionNegations()
        {
            List<IExpressionUnary> programs = new()
            {
                // -1
                UnaryE(LiteralE(1), UOT.ArithmeticNegate),
                // -"a"
                UnaryE(LiteralE("a"), UOT.ArithmeticNegate),
                // -FALSE
                UnaryE(LiteralE(false), UOT.ArithmeticNegate),
                // NOT 0
                UnaryE(LiteralE(0), UOT.LogicalNot),
                // NOT "aba"
                UnaryE(LiteralE("aba"), UOT.LogicalNot),
                // NOT TRUE
                UnaryE(LiteralE(true), UOT.LogicalNot),
            };

            List<object?> expected = new()
            {
                -1.0,  // -1
                null,  // -"a"
                0.0,   // -FALSE
                true,  // NOT 0
                false, // NOT "aba"
                false, // NOT TRUE
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionNoneTest()
        {
            List<IExpressionNoneTest> programs = new()
            {
                // 1 ??
                new ExpressionNoneTest(LiteralE(1)),
                // NONE ??
                new ExpressionNoneTest(LiteralE(null)),
            };

            List<object?> expected = new()
            {
                false,  // 1 ??
                true,  // NONE ??
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionTernary()
        {
            List<IExpressionTernary> programs = new()
            {
                // TRUE ? 1 : 2
                new ExpressionTernary(LiteralE(true), LiteralE(1), LiteralE(2)),
                // FALSE ? 1 : 2
                new ExpressionTernary(LiteralE(false), LiteralE(1), LiteralE(2)),
                // FALSE ? 1
                new ExpressionTernary(LiteralE(false), LiteralE(1), null),
                // TRUE ? TRUE ? 1 : 2 : 3
                new ExpressionTernary(LiteralE(true), new ExpressionTernary(LiteralE(true), LiteralE(1), LiteralE(2)), LiteralE(3)),
                // FALSE ? TRUE ? 1 : 2 : 3
                new ExpressionTernary(LiteralE(false), new ExpressionTernary(LiteralE(true), LiteralE(1), LiteralE(2)), LiteralE(3)),
                // TRUE ? FALSE ? 1 : 2 : 3
                new ExpressionTernary(LiteralE(true), new ExpressionTernary(LiteralE(false), LiteralE(1), LiteralE(2)), LiteralE(3)),
            };

            List<object?> expected = new()
            {
                1.0,  // TRUE ? 1 : 2
                2.0,  // FALSE ? 1 : 2
                null, // FALSE ? 1
                1.0,  // TRUE ? TRUE ? 1 : 2 : 3
                3.0,  // FALSE ? TRUE ? 1 : 2 : 3
                2.0,  // TRUE ? FALSE ? 1 : 2 : 3
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionNoneInTernaryCondition()
        {
            IExpressionTernary programs =
                // NONE ? 1 : 2
                new ExpressionTernary(LiteralE(null), LiteralE(1), LiteralE(2));

            ErrorType expected = ErrorType.ExpectedNotNone;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionPipe()
        {
            List<IExpressionPipe> programs = new()
            {
                // 1 THEN 2 OTHERWISE 3
                new ExpressionPipe(LiteralE(1), LiteralE(2), LiteralE(3)),
                // NONE THEN 2 OTHERWISE 3
                new ExpressionPipe(LiteralE(null), LiteralE(2), LiteralE(3)),
                // NONE THEN 2
                new ExpressionPipe(LiteralE(null), LiteralE(2), null),
                // NONE OTHERWISE 2
                new ExpressionPipe(LiteralE(null), null, LiteralE(2)),
                // 1 OTHERWISE 2
                new ExpressionPipe(LiteralE(1), null, LiteralE(2)),
                // 1 THEN PIPE + 1
                new ExpressionPipe(LiteralE(1), BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(1)), null),
                // {{{1 THEN PIPE + 1} THEN PIPE + 1} THEN PIPE + NONE OTHERWISE 0}
                new ExpressionPipe(
                    new ExpressionPipe(
                        new ExpressionPipe(LiteralE(1),
                            BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(1)),
                            null
                        ),
                        BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(1)),
                        null
                    ),
                    BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(null)),
                    LiteralE(0)
                ),
                // {{{1 THEN PIPE + 1} THEN PIPE + 1 OTHERWISE 0} THEN PIPE + NONE}
                new ExpressionPipe(
                    new ExpressionPipe(
                        new ExpressionPipe(LiteralE(1),
                            BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(1)),
                            null
                        ),
                        BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(1)),
                        LiteralE(0)
                    ),
                    BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(null)),
                    null
                ),
                // {{{1 THEN PIPE + 1} THEN PIPE + 1} THEN PIPE + 1}
                new ExpressionPipe(
                    new ExpressionPipe(
                        new ExpressionPipe(LiteralE(1),
                            BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(1)),
                            null
                        ),
                        BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(1)),
                        null
                    ),
                    BinaryE(IdentifierE("PIPE"), BOT.ArithmeticAdd, LiteralE(1)),
                    null
                ),
            };

            List<object?> expected = new()
            {
                2.0,  // 1 THEN 2 OTHERWISE 3
                3.0,  // NONE THEN 2 OTHERWISE 3
                null, // NONE THEN 2
                2.0,  // NONE OTHERWISE 2
                null, // 1 OTHERWISE 2
                2.0,  // 1 THEN PIPE + 1
                0.0,  // {{{1 THEN PIPE + 1} THEN PIPE + 1} THEN PIPE + NONE OTHERWISE 0}
                null, // {{{1 THEN PIPE + 1} THEN PIPE + 1 OTHERWISE 0} THEN PIPE + NONE}
                4.0,  // {{{1 THEN PIPE + 1} THEN PIPE + 1} THEN PIPE + 1}
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionIdentifierSimple()
        {
            IStatementCompound programs = CompoundS(new()
            {
                // NUMBER a IS 12;
                // a;
                VariableD(false, VT.Number, IdentifierE("a"), LiteralE(12)),
                IdentifierE("a"),
            });

            object? expected = 12;

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionIdentifierOtherScope()
        {
            IStatementCompound programs = CompoundS(new()
            {
                VariableD(false, VT.Number, IdentifierE("a"), LiteralE(12)),
                CompoundS(new() 
                {
                    // NUMBER a IS 12;
                    // BEGIN a; END
                    IdentifierE("a"),
                }),
            });

            object? expected = 12;

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionUndefinedVariable()
        {
            IExpressionIdentifier programs =
                // a;
                IdentifierE("a");

            ErrorType expected = ErrorType.UndefinedVariable;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionInvalidPipeOrValue()
        {
            List<IExpressionIdentifier> programs = new()
            {
                // PIPE;
                IdentifierE("PIPE"),
                // VALUE;
                IdentifierE("VALUE"),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.PipeNotInPipeline,
                ErrorType.ValueNotInMatch,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestExpressionCallBuiltin()
        {
            List<IExpressionCall> programs = new()
            {
                // CALL Print WITH 1 NOW;
                new ExpressionCall(IdentifierE("Print"), new() { LiteralE(1) }),
            };

            List<string> expected = new()
            {
                "1"
            };

            var interpreter = MakeInterpreter(false, false);
            TestInterpreterStdOut(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionCallPattern()
        {
            List<IStatementCompound> programs = new()
            {
                //PATTERN abba WITH TEXT a
                //BEGIN
                //    VALUE === "yes" DO CALL Print WITH a NOW;,
                //    DEFAULT CALL Print WITH "no" NOW;,
                //END
                //CALL abba WITH "yes" NOW;
            CompoundS(new()
                {
                    PatternD(IdentifierE("abba"), Declarator(false, VT.Text, IdentifierE("a")),
                        CompoundS(new()
                        {
                            IfS(BinaryE(IdentifierE("VALUE"), BOT.ComparatorEqualText, LiteralE("yes")),
                                new ExpressionCall(IdentifierE("Print"), new() { IdentifierE("a")}), null),
                            IfS(LiteralE(true),
                                new ExpressionCall(IdentifierE("Print"), new() { LiteralE("no") }), null)
                        })),
                    new ExpressionCall(IdentifierE("abba"), new() { LiteralE("yes") }),
                }),
            };

            List<string> expected = new()
            {
                "yes"
            };

            var interpreter = MakeInterpreter(false, false);
            TestInterpreterStdOut(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionCallUnimplemented()
        {
            List<IStatementCompound> programs = new()
            {
                // FUNCTION a RETURNS NOTHING;
                // CALL a NOW;
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
                    new ExpressionCall(IdentifierE("a"), new() {}),
                }),
                // PATTERN a WITH NUMBER b;
                // CALL a WITH 1 NOW;
                CompoundS(new()
                {
                    PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
                    new ExpressionCall(IdentifierE("a"), new() { LiteralE(1) }),
                }),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.NotImplementedFunction,
                ErrorType.NotImplementedPattern,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestExpressionAssignSimple()
        {
            List<IStatementCompound> programs = new()
            {
                // MUTABLE NUMBER a;
                // a IS 1;
                CompoundS(new()
                {
                    VariableD(true, VT.Number, IdentifierE("a"), null),
                    AssignmentE(LiteralE(1), IdentifierE("a")),
                }),
                // MUTABLE NUMBER a IS 12;
                // a IS 10;
                CompoundS(new()
                {
                    VariableD(true, VT.Number, IdentifierE("a"), LiteralE(12)),
                    AssignmentE(LiteralE(10), IdentifierE("a")),
                }),
                // MUTABLE TEXT a IS 12;
                // a IS TRUE;
                CompoundS(new()
                {
                    VariableD(true, VT.Text, IdentifierE("a"), LiteralE(12)),
                    AssignmentE(LiteralE(true), IdentifierE("a")),
                }),
            };

            List<object?> expected = new()
            {
                1.0,
                10.0,
                "TRUE",
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionAssignNested()
        {
            List<IStatementCompound> programs = new()
            {
                // MUTABLE NUMBER a;
                // MUTABLE NUMBER b;
                // a IS b IS 1;
                CompoundS(new()
                {
                    VariableD(true, VT.Number, IdentifierE("a"), null),
                    VariableD(true, VT.Number, IdentifierE("b"), null),
                    AssignmentE(AssignmentE(LiteralE(1), IdentifierE("b")), IdentifierE("a")),
                }),
                // MUTABLE NUMBER a;
                // MUTABLE TEXT b;
                // MUTABLE FACT c;
                // a IS b IS c IS 1;
                CompoundS(new()
                {
                    VariableD(true, VT.Number, IdentifierE("a"), null),
                    VariableD(true, VT.Text, IdentifierE("b"), null),
                    VariableD(true, VT.Fact, IdentifierE("c"), null),
                    AssignmentE(AssignmentE(AssignmentE(LiteralE(1), IdentifierE("c")), IdentifierE("b")), IdentifierE("a")),
                }),
            };

            List<object?> expected = new()
            {
                1.0,
                null, // a IS b IS c IS 1 => a IS b IS TRUE => a IS "TRUE" => NONE
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestExpressionAssignmentToImmutable()
        {
            List<IStatementCompound> programs = new()
            {
                // NUMBER a;
                // a IS 2;
                CompoundS(new()
                {
                    VariableD(false, VT.Number, IdentifierE("a"), null),
                    AssignmentE(LiteralE(2), IdentifierE("a")),
                }),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.VariableImmutable,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestExpressionAssignmentToPipeOrValue()
        {
            List<IStatementCompound> programs = new()
            {
                // MATCH WITH 1 BEGIN DEFAULT DO VALUE IS 2;, END
                CompoundS(new()
                {
                    new StatementAnonMatch(LiteralE(1), 
                        CompoundS(new() { IfS(LiteralE(true), AssignmentE(LiteralE(2), IdentifierE("VALUE")), null) })),
                }),
                // TRUE THEN PIPE IS 2;
                CompoundS(new()
                {
                    new ExpressionPipe(LiteralE(true), AssignmentE(LiteralE(2), IdentifierE("PIPE")), null),
                }),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.VariableReservedAssignment,
                ErrorType.VariableReservedAssignment,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestExpressionAssignmentToNotVariable()
        {
            List<IExpressionAssignment> programs = new()
            {
                // 1 IS 2;
                AssignmentE(LiteralE(2), LiteralE(1)),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.AssignmentToNotVariable,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestExpressionAssignmentToUndefined()
        {
            List<IExpressionAssignment> programs = new()
            {
                // a IS 2;
                AssignmentE(LiteralE(2), IdentifierE("a")),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.UndefinedVariable,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }
    }
}
