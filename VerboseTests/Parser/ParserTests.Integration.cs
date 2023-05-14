using System.Collections.Generic;

using VerboseCore.Interfaces;
using VerboseCore.Helpers;
using VerboseCore.Parser;

using BOT = VerboseCore.Entities.BinaryOperatorType;
using VT = VerboseCore.Entities.VariableType;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

using static VerboseTests.Parser_.ParserTests;

namespace VerboseTests.Parser_
{
    [TestClass]
    public class IntegrationTests
    {
        private static void TestIntegration(string source, IInstruction? expected)
        {
            var scanner = new ParserScanner(Helpers.MakeStringLexer(source));
            var parser = new Parser(scanner, new MockLogger());
            var actual = parser.BuildProgram();
            actual.Should().BeEquivalentTo(expected, o =>
                o.RespectingRuntimeTypes().AllowingInfiniteRecursion());
        }

        private static void TestIntegration(List<string> sources, List<IInstruction?> expected)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                var scanner = new ParserScanner(Helpers.MakeStringLexer(sources[i]));
                var parser = new Parser(scanner, new MockLogger());
                var actual = parser.BuildProgram();
                actual.Should().BeEquivalentTo(expected[i], o =>
                    o.RespectingRuntimeTypes().AllowingInfiniteRecursion(), i.ToString());
            }
        }

        [TestMethod]
        public void TestParserLexerSimplest()
        {
            string source = "a;";
            IInstruction? expected = CompoundS(new() { IdentifierE("a") });

            TestIntegration(source, expected);
        }

        [TestMethod]
        public void TestParserLexerVariableDeclaration()
        {
            List<string> sources = new()
            { 
                "MUTABLE TEXT a;",
                "NUMBER b IS 1;",
                "FACT cool12 IS 1 + 1;",
                "TEXT z IS 0.11;"
            };
            List<IInstruction?> expected = new()
            {
                CompoundS(new() { VariableD(true, VT.Text, IdentifierE("a"), null) }),
                CompoundS(new() { VariableD(false, VT.Number, IdentifierE("b"), LiteralE(1)) }),
                CompoundS(new() { VariableD(false, VT.Fact, IdentifierE("cool12"), BinaryE(LiteralE(1), BOT.ArithmeticAdd, LiteralE(1)))}),
                CompoundS(new() { VariableD(false, VT.Text, IdentifierE("z"), LiteralE(0.11)) }),
            };

            TestIntegration(sources, expected);
        }
    }
}
