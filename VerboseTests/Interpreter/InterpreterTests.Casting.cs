using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

using static VerboseCLI.Interpreter.Casting;

using static VerboseTests.Interpreter_.InterpreterTests;
using VT = VerboseCore.Entities.VariableType;

namespace VerboseTests.Interpreter_
{
    [TestClass]
    public class CastingTests
    {
        [TestMethod]
        public void TestCastNumber()
        {
            List<object?> values = new()
            {
                1.0,
                "1",
                "-1.20",
                "10___0",
                "0b111",
                "0xa",
                true,
                false,
                null,
            };

            List<double?> expected = new()
            {
                1.0,
                1.0,
                -1.20,
                100.0,
                7.0,
                10.0,
                1.0,
                0.0,
                null,
            };

            TestInterpreterCasting(values, expected, AsNumber);
        }

        [TestMethod]
        public void TestCastText()
        {
            List<object?> values = new()
            {
                1.0,
                -1.5,
                "1",
                "aaabbbCCC",
                true,
                false,
                null,
            };

            List<string?> expected = new()
            {
                "1",
                "-1.5",
                "1",
                "aaabbbCCC",
                "TRUE",
                "FALSE",
                null,
            };

            TestInterpreterCasting(values, expected, AsText);
        }

        [TestMethod]
        public void TestCastFact()
        {
            List<object?> values = new()
            {
                1.0,
                1.5,
                0.0,
                "aaa",
                "",
                true,
                false,
                null,
            };

            List<bool?> expected = new()
            {
                true,
                true,
                false,
                true,
                false,
                true,
                false,
                null,
            };

            TestInterpreterCasting(values, expected, AsFact);
        }

        [TestMethod]
        public void TestCastAsType()
        {
            List<object?> values = new()
            {
                1234567890.0,
                "-123__{.}0",
                true,
                -15.5555555,
                "anna",
                false,
                12.0,
                "ą",
                false,
                null,
            };

            List<VT> types = new()
            {
                VT.Number,
                VT.Number,
                VT.Number,
                VT.Text,
                VT.Text,
                VT.Text,
                VT.Fact,
                VT.Fact,
                VT.Fact,
                VT.Fact,
            };

            List<object?> expected = new()
            {
                1234567890.0,
                -123.0,
                1.0,
                "-15.5555555",
                "anna",
                "FALSE",
                true,
                true,
                false,
                null,
            };

            TestInterpreterCastingAs(values, types, expected, AsType);
        }
    }
}
