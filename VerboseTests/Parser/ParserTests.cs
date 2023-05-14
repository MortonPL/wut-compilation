using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

using System;
using System.Collections.Generic;

using VerboseCore.Abstract;
using VerboseCore.Entities;
using VerboseCore.Interfaces;

using TT = VerboseCore.Entities.TokenType;

namespace VerboseTests.Parser_
{
    [TestClass]
    public partial class ParserTests
    {
        public class MockParserScanner : IParserScanner
        {
            public APosition Position { get; set; } = new Position();
            public IToken Buffer { get => _buffer; }
            public string BufferedError { get; } = "";
            public TT Type { get => _buffer.Type; }
            public APosition TPosition { get => _buffer.Position; }
            public object? Value { get => _buffer.Value; }

            private IToken _buffer = new Token(TT.SpecialNUL, new Position());
            private List<IToken> _tokens = new();
            private int index = 0;

            public MockParserScanner() { }

            public void Load(List<IToken> tokens)
            {
                index = 0;
                _tokens = tokens;
                Next();
            }

            public void Next()
            {
                _buffer = index == _tokens.Count ? new Token(TT.SpecialETX, new Position()) : _tokens[index++];
            }
        }

        public static void TestInstructions(MockParserScanner scanner, List<List<IToken>> tokenLists, List<IInstruction?> expected, Func<IInstruction?> method)
        {
            for (int i = 0; i < tokenLists.Count; i++)
            {
                scanner.Load(tokenLists[i]);
                IInstruction? actual = method();
                actual.Should().BeEquivalentTo(expected[i], o =>
                    o.RespectingRuntimeTypes().AllowingInfiniteRecursion(), i.ToString());
            }
        }

        public static void TestInstructions(MockParserScanner scanner, List<IToken> tokenList, IInstruction? expected, Func<IInstruction?> method)
        {
            scanner.Load(tokenList);
            IInstruction? actual = method();
            actual.Should().BeEquivalentTo(expected, o =>
                o.RespectingRuntimeTypes().AllowingInfiniteRecursion());
        }
    }
}
