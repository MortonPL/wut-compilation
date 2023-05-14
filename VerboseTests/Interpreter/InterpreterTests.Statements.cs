using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Interfaces;
using VerboseCLI.Entities;

using static VerboseTests.Parser_.ParserTests;
using static VerboseTests.Interpreter_.InterpreterTests;
using BOT = VerboseCore.Entities.BinaryOperatorType;

namespace VerboseTests.Interpreter_
{
    [TestClass]
    public class StatementTests
    {
        [TestMethod]
        public void TestStatementIf()
        {
            List<IStatementIf> programs = new()
            {
                // IF TRUE DO 1 ELSE 2;
                IfS(LiteralE(true), LiteralE(1), LiteralE(2)),
                // IF FALSE DO 1 ELSE 2;
                IfS(LiteralE(false), LiteralE(1), LiteralE(2)),
                // IF FALSE DO 1 ELSE IF TRUE DO 2;
                IfS(LiteralE(false), LiteralE(1), IfS(LiteralE(true), LiteralE(2), null)),
                // IF TRUE DO BEGIN 1; 2; 3; 4; END;
                IfS(LiteralE(true), CompoundS(new() { LiteralE(1), LiteralE(2), LiteralE(3), LiteralE(4)}), null),
            };

            List<object?> expected = new()
            {
                1.0,  // IF TRUE DO 1 ELSE 2;
                2.0,  // IF FALSE DO 1 ELSE 2;
                2.0,  // IF FALSE DO 1 ELSE IF TRUE DO 2;
                4.0,  // IF TRUE DO BEGIN 1; 2; 3; 4; END;
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestStatementIfNoConditionError()
        {
            IStatementIf programs =
                // IF NONE DO 1 ELSE 2;
                IfS(LiteralE(null), LiteralE(1), LiteralE(2));

            ErrorType expected = ErrorType.ExpectedNotNone;  // IF NONE DO 1 ELSE 2;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestStatementWhile ()
        {
            List<IStatementCompound> programs = new()
            {
                // BEGIN WHILE TRUE DO 1; STOP; END
                CompoundS(new()
                {
                    new StatementWhile(LiteralE(true), CompoundS(new() { LiteralE(1), new StatementJump(JumpType.Stop, null) })),
                }),
                // BEGIN
                // MUTABLE NUMBER a IS 4;
                // WHILE a > 0 DO a IS a - 1;
                // END
                CompoundS(new()
                {
                    VariableD(true, VariableType.Number, IdentifierE("a"), LiteralE(4)),
                    new StatementWhile(BinaryE(IdentifierE("a"), BOT.ComparatorGreater, LiteralE(0)),
                        AssignmentE(BinaryE(IdentifierE("a"), BOT.ArithmeticSub, LiteralE(1)), IdentifierE("a"))),
                }),
            };

            List<object?> expected = new()
            {
                true,
                false,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestStatementSkipOrStopOutsideLoop()
        {
            List<IStatementCompound> programs = new()
            {
                // SKIP;
                CompoundS(new() { new StatementJump(JumpType.Skip, null) }),
                // STOP;
                CompoundS(new() { new StatementJump(JumpType.Stop, null) }),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.InvalidSkip,
                ErrorType.InvalidStop,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestStatementUnexpectedReturnValue()
        {
            IStatementCompound programs =
                // RETURN 1;
                CompoundS(new() { new StatementJump(JumpType.Return, LiteralE(1.0)) });

            ErrorType expected = ErrorType.ReturnUnexpectedValue;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        // in this scenario we artificially force return type, for convenience
        [TestMethod]
        public void TestStatementJumpInPatternOrMatch()
        {
            IStatementJump programs =
                // RETURN;
                new StatementJump(JumpType.Return, null);

            ErrorType expected = ErrorType.JumpInMatch;

            var interpreter = MakeInterpreter();
            interpreter.CurrentContext.SetFlag(ScopeFlags.Match);
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        // in this scenario we artificially force return type, for convenience
        [TestMethod]
        public void TestStatementExpectedReturnValue()
        {
            IStatementCompound programs =
                // RETURN;
                CompoundS(new() { new StatementJump(JumpType.Return, null) });

            ErrorType expected = ErrorType.ReturnExpectedValue;

            var interpreter = MakeInterpreter();
            interpreter.CurrentContext.ReturnType = VariableType.Text;
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestStatementAnonMatch()
        {
            List<IStatementAnonMatch> programs = new()
            {
                // MATCH WITH 1 BEGIN DEFAULT DO 2;, END
                new StatementAnonMatch(LiteralE(1), CompoundS(new() { IfS(LiteralE(true), LiteralE(2), null)})),
                // MATCH WITH 1
                // BEGIN
                // FALSE DO 1;,
                // VALUE + "1" DO 4;,
                // DEFAULT DO 3;, END
                new StatementAnonMatch(LiteralE(1), CompoundS(new()
                {
                    IfS(LiteralE(false), LiteralE(1), null),
                    IfS(BinaryE(IdentifierE("VALUE"), BOT.ArithmeticAdd, LiteralE("1")), LiteralE(4), null),
                    IfS(LiteralE(true), LiteralE(3), null),
                })),
            };

            List<object?> expected = new()
            {
                2.0,
                4.0,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterExpression(interpreter, programs, expected, interpreter.Visit);
        }
    }
}
