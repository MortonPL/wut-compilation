using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

using VerboseCore.Helpers;
using VerboseCore.Interfaces;
using VerboseCore.Entities;
using static VerboseTests.Parser_.ParserTests;
using static VerboseTests.Interpreter_.InterpreterTests;

using VerboseCLI.Entities;
using VerboseCLI.Interpreter;

using VT = VerboseCore.Entities.VariableType;

namespace VerboseTests.Interpreter_
{
    [TestClass]
    public class DeclarationTests
    {
        [TestMethod]
        public void TestVariableDeclarationSimple()
        {
            List<IDeclarationVariable> programs = new()
            {
                // NUMBER a;
                VariableD(false, VT.Number, IdentifierE("a"), null),
                // MUTABLE NUMBER a;
                VariableD(true, VT.Number, IdentifierE("a"), null),
                // MUTABLE NUMBER a IS 12;
                VariableD(true, VT.Number, IdentifierE("a"), LiteralE(12)),
            };

            List<Context> expected = new()
            {
                MakeGlobalContextAndUnpack(MakeVariableScope(false, "a", VT.Number, null)),
                MakeGlobalContextAndUnpack(MakeVariableScope(true, "a", VT.Number, null)),
                MakeGlobalContextAndUnpack(MakeVariableScope(true, "a", VT.Number, 12)),
            };

            var interpreter = MakeInterpreter();
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestVariableDeclarationAllTypes()
        {
            List<IDeclarationVariable> programs = new()
            {
                // NUMBER a IS 10;
                VariableD(false, VT.Number, IdentifierE("a"), LiteralE(10)),
                // NUMBER a IS "12.5";
                VariableD(false, VT.Number, IdentifierE("a"), LiteralE("12.5")),
                // NUMBER a IS TRUE;
                VariableD(false, VT.Number, IdentifierE("a"), LiteralE(true)),
                // TEXT a IS 10;
                VariableD(false, VT.Text, IdentifierE("a"), LiteralE(10)),
                // TEXT a IS "12.5";
                VariableD(false, VT.Text, IdentifierE("a"), LiteralE("12.5")),
                // TEXT a IS TRUE;
                VariableD(false, VT.Text, IdentifierE("a"), LiteralE(true)),
                // FACT a IS 10;
                VariableD(false, VT.Fact, IdentifierE("a"), LiteralE(10)),
                // FACT a IS "12.5";
                VariableD(false, VT.Fact, IdentifierE("a"), LiteralE("12.5")),
                // FACT a IS TRUE;
                VariableD(false, VT.Fact, IdentifierE("a"), LiteralE(true)),
            };

            List<Context> expected = new()
            {
                MakeGlobalContextAndUnpack(MakeVariableScope(false, "a", VT.Number, 10.0)),
                MakeGlobalContextAndUnpack(MakeVariableScope(false, "a", VT.Number, 12.5)),
                MakeGlobalContextAndUnpack(MakeVariableScope(false, "a", VT.Number, 1.0)),
                MakeGlobalContextAndUnpack(MakeVariableScope(true, "a", VT.Text, "10")),
                MakeGlobalContextAndUnpack(MakeVariableScope(true, "a", VT.Text, "12.5")),
                MakeGlobalContextAndUnpack(MakeVariableScope(true, "a", VT.Text, "TRUE")),
                MakeGlobalContextAndUnpack(MakeVariableScope(true, "a", VT.Fact, true)),
                MakeGlobalContextAndUnpack(MakeVariableScope(true, "a", VT.Fact, true)),
                MakeGlobalContextAndUnpack(MakeVariableScope(true, "a", VT.Fact, true)),
            };

            var interpreter = MakeInterpreter();
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestVariableDeclarationAlreadyDeclared()
        {
            IStatementCompound programs = CompoundS(new()
            {
                VariableD(false, VT.Text, IdentifierE("a"), LiteralE(10)),
                VariableD(false, VT.Number, IdentifierE("a"), null),
            });

            ErrorType expected = ErrorType.VariableRedefinition;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestVariableDeclarationReservedVariables()
        {
            List<IDeclarationVariable> programs = new()
            {
                VariableD(false, VT.Text, IdentifierE("PIPE"), LiteralE(10)),
                VariableD(false, VT.Number, IdentifierE("VALUE"), null),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.VariableReserved,
                ErrorType.VariableReserved,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestFunctionDeclarationSimple()
        {
            List<IDeclarationFunction> programs = new()
            {
                // FUNCTION a RETURNS NOTHING;
                FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
                // FUNCTION a WITH TEXT b RETURNS TEXT;
                FunctionD(VT.Text, IdentifierE("a"), new() {Declarator(false, VT.Text, IdentifierE("b"))}, new StatementEmpty()),
                // FUNCTION a WITH TEXT b FACT c RETURNS TEXT b;
                FunctionD(VT.Text, IdentifierE("a"), new() { Declarator(false, VT.Text, IdentifierE("b")), Declarator(false, VT.Fact, IdentifierE("c")) },
                    IdentifierE("b")),
            };

            List<Context> expected = new()
            {
                MakeGlobalContextAndUnpack(MakeFunctionScope(programs[0])),
                MakeGlobalContextAndUnpack(MakeFunctionScope(programs[1])),
                MakeGlobalContextAndUnpack(MakeFunctionScope(programs[2])),
            };

            var interpreter = MakeInterpreter();
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestFunctionDeclarationMultiple()
        {
            IStatementCompound programs =
                // BEGIN
                // FUNCTION a RETURNS NOTHING;
                // FUNCTION a WITH TEXT c RETURNS NOTHING;
                // END
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
                    FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(false, VT.Text, IdentifierE("c")) }, new StatementEmpty()),
                });

            Context expected =
                MakeGlobalContextAndUnpack(MakeFunctionScope(new()
                {
                    new(FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()), false),
                    new(FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(false, VT.Text, IdentifierE("c")) }, new StatementEmpty()), false),
                }));

            var interpreter = MakeInterpreter();
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestFunctionDeclarationDifferentScopes()
        {
            IStatementCompound programs =
            // BEGIN
            //   FUNCTION a RETURNS NOTHING;
            //   BEGIN
            //     FUNCTION a WITH TEXT c RETURNS NOTHING;
            //   END
            // END
            CompoundS(new()
            {
                FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(false, VT.Text, IdentifierE("c")) }, new StatementEmpty()),
                }),
            });

            Context expected = Interpreter.MakeGlobalContext();
            expected._scopes.Add(MakeFunctionScope(FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()), false));
            expected._scopes.Add(MakeFunctionScope(FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(false, VT.Text, IdentifierE("c")) }, new StatementEmpty()), false));

            var interpreter = MakeInterpreter(true);
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestFunctionDeclarationAlreadyDefined()
        {
            List<IStatementCompound> programs = new()
            {
                // BEGIN
                // FUNCTION a RETURNS NOTHING 1;
                // FUNCTION a RETURNS NOTHING 1;
                // END
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new(), LiteralE(1)),
                    FunctionD(VT.Nothing, IdentifierE("a"), new(), LiteralE(1)),
                }),
                // BEGIN
                // FUNCTION a RETURNS NOTHING 1;
                // FUNCTION a RETURNS NOTHING;
                // END
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new(), LiteralE(1)),
                    FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
                }),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.FunctionRedefinition,
                ErrorType.FunctionRedefinition,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestFunctionDeclarationAlreadyDeclared()
        {
            // BEGIN
            // FUNCTION a RETURNS NOTHING;
            // FUNCTION a RETURNS NOTHING;
            // END
            IStatementCompound programs = CompoundS(new()
            {
                FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
                FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
            });

            ErrorType expected = ErrorType.FunctionRedeclaration;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestFunctionBuiltinRedefinition()
        {
            // BEGIN
            // FUNCTION Print WITH TEXT t RETURNS NOTHING;
            // END
            IDeclarationFunction programs =
                FunctionD(VT.Nothing, IdentifierE("Print"), new() { Declarator(false, VT.Text, IdentifierE("t"))}, new StatementEmpty());

            ErrorType expected = ErrorType.BuiltinRedefinition;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }


        [TestMethod]
        public void TestFunctionDefinitionSoftOverride()
        {
            IStatementCompound programs =
            // BEGIN
            //   FUNCTION a RETURNS NOTHING;
            //   BEGIN
            //     FUNCTION a RETURNS NOTHING 1;
            //   END
            // END
            CompoundS(new()
            {
                FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new(), LiteralE(1)),
                }),
            });

            Context expected = Interpreter.MakeGlobalContext();
            expected._scopes.Add(MakeFunctionScope(FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()), false));
            expected._scopes.Add(MakeFunctionScope(FunctionD(VT.Nothing, IdentifierE("a"), new(), LiteralE(1)), false));

            var interpreter = MakeInterpreter(true);
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestFunctionDefinitionHardOverride()
        {
            IStatementCompound programs =
            // BEGIN
            //   FUNCTION a RETURNS NOTHING;
            //   BEGIN
            //     FUNCTION OVERRIDE a RETURNS NOTHING 1;
            //   END
            // END
            CompoundS(new()
            {
                FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new(), LiteralE(1), true),
                }),
            });

            Context expected = Interpreter.MakeGlobalContext();
            expected._scopes.Add(MakeFunctionScope(FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()), false));
            expected._scopes.Add(new Scope());

            var interpreter = MakeInterpreter(true);
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestFunctionDefinitionBadOverride()
        {
            IStatementCompound programs =
            //   BEGIN
            //     FUNCTION OVERRIDE a RETURNS NOTHING 1;
            //   END
            CompoundS(new()
            {
                FunctionD(VT.Nothing, IdentifierE("a"), new(), LiteralE(1), true),
            });

            ErrorType expected = ErrorType.BadFunctionOverride;

            var interpreter = MakeInterpreter(true);
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestFunctionDefinitionReturnMismtach()
        {
            // BEGIN
            // FUNCTION a RETURNS NOTHING;
            // FUNCTION a RETURNS TEXT 1;
            // END
            IStatementCompound programs = CompoundS(new()
            {
                FunctionD(VT.Nothing, IdentifierE("a"), new(), new StatementEmpty()),
                FunctionD(VT.Text, IdentifierE("a"), new(), LiteralE(1)),
            });

            ErrorType expected = ErrorType.FunctionDefinitionReturnMismatch;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestFunctionDefinitionParamsMismatch()
        {
            List<IStatementCompound> programs = new()
            {
                // BEGIN
                // FUNCTION a WITH TEXT a RETURNS NOTHING;
                // FUNCTION a WITH TEXT b RETURNS NOTHING 1;
                // END
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(false, VT.Text, IdentifierE("a")) }, new StatementEmpty()),
                    FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(false, VT.Text, IdentifierE("b")) }, LiteralE(1)),
                }),
                // BEGIN
                // FUNCTION a WITH TEXT a RETURNS NOTHING;
                // FUNCTION a WITH MUTABLE TEXT a RETURNS NOTHING 1;
                // END
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(false, VT.Text, IdentifierE("a")) }, new StatementEmpty()),
                    FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(true, VT.Text, IdentifierE("a")) }, LiteralE(1)),
                }),
                // BEGIN
                // FUNCTION a WITH TEXT a RETURNS NOTHING;
                // FUNCTION a WITH FACT a RETURNS NOTHING 1;
                // END
                CompoundS(new()
                {
                    FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(false, VT.Text, IdentifierE("a")) }, new StatementEmpty()),
                    FunctionD(VT.Nothing, IdentifierE("a"), new() { Declarator(false, VT.Fact, IdentifierE("a")) }, LiteralE(1)),
                }),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.FunctionDefinitionParamTypeMismatch,
                ErrorType.FunctionDefinitionParamTypeMismatch,
                ErrorType.FunctionDefinitionParamTypeMismatch,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestPatternDeclarationSimple()
        {
            List<IDeclarationPattern> programs = new()
            {
                // PATTERN a WITH NUMBER b;
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
                // PATTERN b WITH MUTABLE NUMBER c;
                PatternD(IdentifierE("a"), Declarator(true, VT.Number, IdentifierE("b")), new StatementEmpty()),
            };

            List<Context> expected = new()
            {
                MakeGlobalContextAndUnpack(MakePatternScope(programs[0])),
                MakeGlobalContextAndUnpack(MakePatternScope(programs[1])),
            };

            var interpreter = MakeInterpreter();
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestPatternDeclarationMultiple()
        {
            IStatementCompound programs =
                // BEGIN
                // PATTERN a WITH NUMBER b;
                // PATTERN b WITH MUTABLE NUMBER c;
                // END
                CompoundS(new()
                {
                    PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
                    PatternD(IdentifierE("b"), Declarator(true, VT.Number, IdentifierE("c")), new StatementEmpty()),
                });

            Context expected =
                MakeGlobalContextAndUnpack(MakePatternScope(new()
                {
                    new(PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()), false),
                    new(PatternD(IdentifierE("b"), Declarator(true, VT.Number, IdentifierE("c")), new StatementEmpty()), false),
                }));

            var interpreter = MakeInterpreter();
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestPatternDeclarationDifferentScopes()
        {
            IStatementCompound programs =
            // BEGIN
            //   PATTERN a WITH NUMBER b;
            //   BEGIN
            //     PATTERN b WITH MUTABLE NUMBER c;
            //   END
            // END
            CompoundS(new()
            {
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
                CompoundS(new()
                {
                    PatternD(IdentifierE("b"), Declarator(true, VT.Number, IdentifierE("c")), new StatementEmpty()),
                }),
            });

            Context expected = Interpreter.MakeGlobalContext();
            expected._scopes.Add(MakePatternScope(PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()), false));
            expected._scopes.Add(MakePatternScope(PatternD(IdentifierE("b"), Declarator(true, VT.Number, IdentifierE("c")), new StatementEmpty()), false));

            var interpreter = MakeInterpreter(true);
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit);
        }

        public void TestPatternDeclarationAlreadyDefined()
        {
            List<IStatementCompound> programs = new()
            {
                // BEGIN
                // PATTERN a WITH NUMBER b BEGIN DEFAULT 1;, END
                // PATTERN a WITH NUMBER b BEGIN DEFAULT 1;, END
                // END
                CompoundS(new()
                {
                    PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) })),
                    PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) })),
                }),
                // BEGIN
                // PATTERN a WITH NUMBER b BEGIN DEFAULT 1;, END
                // PATTERN a WITH NUMBER b;
                // END
                CompoundS(new()
                {
                    PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) })),
                    PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
                }),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.PatternRedefinition,
                ErrorType.PatternRedefinition,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }

        [TestMethod]
        public void TestPatternDeclarationAlreadyDeclared()
        {
            // BEGIN
            // PATTERN a WITH NUMBER b;
            // PATTERN a WITH NUMBER b;
            // END
            IStatementCompound programs = CompoundS(new()
            {
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
            });

            ErrorType expected = ErrorType.PatternRedeclaration;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestPatternBuiltinRedefinition()
        {
            // BEGIN
            // PATTERN FizzBuzz WITH NUMBER n BEGIN DEFAULT 1;, END
            // END
            IDeclarationPattern programs =
                PatternD(IdentifierE("FizzBuzz"), Declarator(false, VT.Number, IdentifierE("n")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) }));

            ErrorType expected = ErrorType.BuiltinRedefinition;

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestPatternDefinitionSoftOverride()
        {
            IStatementCompound programs =
            // BEGIN
            //   PATTERN a WITH NUMBER b;
            //   BEGIN
            //     PATTERN a WITH NUMBER b BEGIN DEFAULT 1;, END
            //   END
            // END
            CompoundS(new()
            {
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
                CompoundS(new()
                {
                    PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) })),
                }),
            });

            Context expected = Interpreter.MakeGlobalContext();
            expected._scopes.Add(MakePatternScope(
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty())));
            expected._scopes.Add(MakePatternScope(
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) }))));

            var interpreter = MakeInterpreter(true);
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestPatternDefinitionHardOverride()
        {
            IStatementCompound programs =
            // BEGIN
            //   PATTERN a WITH NUMBER b;
            //   BEGIN
            //     PATTERN OVERRIDE a WITH NUMBER b BEGIN DEFAULT 1;, END
            //   END
            // END
            CompoundS(new()
            {
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
                CompoundS(new()
                {
                    PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) }), true),
                }),
            });

            Context expected = Interpreter.MakeGlobalContext();
            expected._scopes.Add(MakePatternScope(
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) })), false));
            expected._scopes.Add(new Scope());

            var interpreter = MakeInterpreter(true);
            TestInterpreterDeclaration(interpreter, programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestPatternDefinitionBadOverride()
        {
            IStatementCompound programs =
            //   BEGIN
            //     PATTERN OVERRIDE a WITH NUMBER b BEGIN DEFAULT 1;, END
            //   END
            CompoundS(new()
            {
                PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) }), true),
            });

            ErrorType expected = ErrorType.BadPatternOverride;

            var interpreter = MakeInterpreter(true);
            TestInterpreterException(programs, expected, interpreter.Visit);
        }

        [TestMethod]
        public void TestPatternDefinitionParamsMismatch()
        {
            List<IStatementCompound> programs = new()
            {
                // BEGIN
                // PATTERN a WITH NUMBER b;
                // PATTERN a WITH TEXT b BEGIN DEFAULT 1;, END
                // END
                CompoundS(new()
                {
                    PatternD(IdentifierE("a"), Declarator(false, VT.Number, IdentifierE("b")), new StatementEmpty()),
                    PatternD(IdentifierE("a"), Declarator(false, VT.Text, IdentifierE("b")), CompoundS(new() { IfS(LiteralE(true), LiteralE(1), null) }), true),
                }),
            };

            List<ErrorType> expected = new()
            {
                ErrorType.PatternDefinitionParamTypeMismatch,
            };

            var interpreter = MakeInterpreter();
            TestInterpreterException(programs, expected, interpreter.Visit, interpreter.Reset);
        }
    }
}
