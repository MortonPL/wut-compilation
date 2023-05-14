using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Interfaces;
using VerboseCore.Helpers;
using VerboseCore.Parser;

using static VerboseTests.Parser_.ParserTests;

using BOT = VerboseCore.Entities.BinaryOperatorType;
using VT = VerboseCore.Entities.VariableType;
using JT = VerboseCore.Entities.JumpType;

namespace VerboseTests.Parser_
{
    [TestClass]
    public class StatementTests
    {
        // jump_statement = SKIP | STOP | RETURN | (RETURN, expression), DOT;
        [TestMethod]
        public void TestJumpStatement()
        {
            List<List<IToken>> tokenLists = new()
            {
                // a
                new() { IdentifierT("a") },
                // SKIP;
                new() { KeywordT("SKIP"), DotT() },
                // STOP;
                new() { KeywordT("STOP"), DotT() },
                // RETURN;
                new() { KeywordT("RETURN"), DotT() },
                // RETURN a;
                new() { KeywordT("RETURN"), IdentifierT("a"), DotT() },
            };
            List<IInstruction?> expected = new()
            {
                null,
                new StatementJump(JT.Skip, null),
                new StatementJump(JT.Stop, null),
                new StatementJump(JT.Return, null),
                new StatementJump(JT.Return, IdentifierE("a")),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildJumpStatement);
        }

        // while_statement = WHILE, expression, DO, statement;
        [TestMethod]
        public void TestWhileStatement()
        {
            List<List<IToken>> tokenLists = new()
            {
                // a;
                new() { IdentifierT("a") },
                // WHILE a DO 1;
                new()
                {
                    KeywordT("WHILE"),
                    IdentifierT("a"),
                    KeywordT("DO"),
                    LiteralT(1),
                    DotT()
                },
            };
            List<IInstruction?> expected = new()
            {
                null,
                new StatementWhile(IdentifierE("a"), LiteralE(1)),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildWhileStatement);
        }

        // for_statement = FOR, variable_declaration, while_statement;
        [TestMethod]
        public void TestForStatement()
        {
            List<List<IToken>> tokenLists = new()
            {
                // a;
                TrailDot(new() { IdentifierT("a") }),
                // FOR MUTABLE NUMBER a IS 1 WHILE a DO b;
                new()
                {
                    KeywordT("FOR"),
                    KeywordT("MUTABLE"),
                    KeywordT("NUMBER"),
                    IdentifierT("a"),
                    KeywordT("IS"),
                    LiteralT(1),
                    KeywordT("WHILE"),
                    IdentifierT("a"),
                    KeywordT("DO"),
                    IdentifierT("b"),
                    DotT(),
                },
            };
            List<IInstruction?> expected = new()
            {
                null,
                CompoundS(new()
                {
                    VariableD(true, VT.Number, IdentifierE("a"), LiteralE(1)),
                    new StatementWhile(IdentifierE("a"), IdentifierE("b")),
                }),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildForStatement);
        }

        // if_statement = IF, expression, DO, statement, [ELSE, statement];
        [TestMethod]
        public void TestIfStatementSimple()
        {
            List<List<IToken>> tokenLists = new()
            {
                // a
                new() { IdentifierT("a") },
                // IF a DO b;
                new()
                {
                    KeywordT("IF"),
                    IdentifierT("a"),
                    KeywordT("DO"),
                    IdentifierT("b"),
                    DotT(),
                },
            };
            List<IInstruction?> expected = new()
            {
                null,
                new StatementIf(IdentifierE("a"), IdentifierE("b"), null),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildIfStatement);
        }

        // if_statement = IF, expression, DO, statement, [ELSE, statement];
        [TestMethod]
        public void TestIfStatementElse()
        {
            List<List<IToken>> tokenList = new()
            {
                // a
                new() { IdentifierT("a") },
                // IF a DO b; ELSE c;
                new()
                {
                    KeywordT("IF"),
                    IdentifierT("a"),
                    KeywordT("DO"),
                    IdentifierT("b"),
                    DotT(),
                    KeywordT("ELSE"),
                    IdentifierT("c"),
                    DotT(),
                }
            };
            List<IInstruction?> expected = new()
            {
                null,
                new StatementIf(IdentifierE("a"), IdentifierE("b"), IdentifierE("c")),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildIfStatement);
        }

        // if_statement = IF, expression, DO, statement, [ELSE, statement];
        [TestMethod]
        public void TestIfStatementMultiple()
        {
            List<List<IToken>> tokenList = new()
            {
                // a
                new() { IdentifierT("a") },
                // IF a DO b; ELSE IF c DO d ELSE e;
                new()
                {
                    KeywordT("IF"),
                    IdentifierT("a"),
                    KeywordT("DO"),
                    IdentifierT("b"),
                    DotT(),
                    KeywordT("ELSE"),
                    KeywordT("IF"),
                    IdentifierT("c"),
                    KeywordT("DO"),
                    IdentifierT("d"),
                    DotT(),
                    KeywordT("ELSE"),
                    IdentifierT("e"),
                    DotT(),
                }
            };
            List<IInstruction?> expected = new()
            {
                null,
                new StatementIf(IdentifierE("a"), IdentifierE("b"),
                    new StatementIf(IdentifierE("c"), IdentifierE("d"), IdentifierE("e"))
                )
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildIfStatement);
        }

        // anon_match = MATCH, WITH, expression, match_block;
        // match_block = BEGIN, {match_branch}, DEFAULT, statement, COMMA, END;
        // match_branch = expression, DO, statement, COMMA;
        [TestMethod]
        public void TestAnonMatchStatementSingleBranch()
        {
            List<List<IToken>> tokenLists = new()
            {
                // a;
                new() { IdentifierT("a") },
                // MATCH WITH a BEGIN
                // VALUE DO 1;,
                // DEFAULT 2;,
                // END
                new()
                {
                    KeywordT("MATCH"),
                    KeywordT("WITH"),
                    IdentifierT("a"),
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
                },
            };
            List<IInstruction?> expected = new()
            {
                null,
                new StatementAnonMatch(IdentifierE("a"), CompoundS(new()
                {
                    IfS(IdentifierE("VALUE"), LiteralE(1), null),
                    IfS(LiteralE(true), LiteralE(2), null),
                })),
            };

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenLists, expected, parser.BuildAnonMatchStatement);
        }

        // anon_match = MATCH, WITH, expression, match_block;
        // match_block = BEGIN, {match_branch}, DEFAULT, statement, COMMA, END;
        // match_branch = expression, DO, statement, COMMA;
        [TestMethod]
        public void TestAnonMatchStatementDefault()
        {
            List<IToken> tokenList = new()
            {
                // MATCH WITH a BEGIN
                // DEFAULT 2;,
                // END
                KeywordT("MATCH"),
                KeywordT("WITH"),
                IdentifierT("a"),
                KeywordT("BEGIN"),
                KeywordT("DEFAULT"),
                LiteralT(2),
                DotT(),
                OperatorT(","),
                KeywordT("END")
            };

            IInstruction? expected = new StatementAnonMatch(IdentifierE("a"), CompoundS(new()
            {
                IfS(LiteralE(true), LiteralE(2), null),
            }));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildAnonMatchStatement);
        }

        // anon_match = MATCH, WITH, expression, match_block;
        // match_block = BEGIN, {match_branch}, DEFAULT, statement, COMMA, END;
        // match_branch = expression, DO, statement, COMMA;
        [TestMethod]
        public void TestAnonMatchStatementMultiBranch()
        {
            List<IToken> tokenList = new()
            {
                // MATCH WITH a BEGIN
                // VALUE DO 1;,
                // b DO 2;,
                // DEFAULT 3;,
                // END
                KeywordT("MATCH"),
                KeywordT("WITH"),
                IdentifierT("a"),
                KeywordT("BEGIN"),
                IdentifierT("VALUE"),
                KeywordT("DO"),
                LiteralT(1),
                DotT(),
                OperatorT(","),
                IdentifierT("b"),
                KeywordT("DO"),
                LiteralT(2),
                DotT(),
                OperatorT(","),
                KeywordT("DEFAULT"),
                LiteralT(3),
                DotT(),
                OperatorT(","),
                KeywordT("END")
            };
            IInstruction? expected = new StatementAnonMatch(IdentifierE("a"), CompoundS(new()
            {
                IfS(IdentifierE("VALUE"), LiteralE(1), null),
                IfS(IdentifierE("b"), LiteralE(2), null),
                IfS(LiteralE(true), LiteralE(3), null),
            }));

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildAnonMatchStatement);
        }

        // compound_statement = BEGIN, {declaration | statement}, END;
        [TestMethod]
        public void TestCompoundStatementOnlyStatements()
        {
            List<IToken> tokenList = new()
            {
                // BEGIN a IS b; b IS c; END
                BeginT(),
                IdentifierT("a"),
                KeywordT("IS"),
                IdentifierT("b"),
                DotT(),
                IdentifierT("b"),
                KeywordT("IS"),
                IdentifierT("c"),
                DotT(),
                EndT()
            };
            IInstruction? expected = CompoundS(new()
            {
                AssignmentE(IdentifierE("b"), IdentifierE("a")),
                AssignmentE(IdentifierE("c"), IdentifierE("b")),
            });

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildCompoundStatement);
        }

        // compound_statement = BEGIN, {declaration | statement}, END;
        [TestMethod]
        public void TestCompoundStatementNegativeA()
        {
            List<IToken> tokenList = new()
            {
                // a
                IdentifierT("a"),
            };
            IInstruction? expected = null;

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildCompoundStatement);
        }

        // compound_statement = BEGIN, {declaration | statement}, END;
        [TestMethod]
        public void TestCompoundStatementOnlyDeclarations()
        {
            List<IToken> tokenList = new()
            {
                // BEGIN MUTABLE NUMBER a IS 1; TEXT b; END
                BeginT(),
                KeywordT("MUTABLE"),
                KeywordT("NUMBER"),
                IdentifierT("a"),
                KeywordT("IS"),
                LiteralT(1),
                DotT(),
                KeywordT("TEXT"),
                IdentifierT("b"),
                DotT(),
                EndT()
            };
            IInstruction? expected = CompoundS(new()
            {
                VariableD(true, VT.Number, IdentifierE("a"), LiteralE(1)),
                VariableD(false, VT.Text, IdentifierE("b"), null),
            });

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildCompoundStatement);
        }

        // compound_statement = BEGIN, {declaration | statement}, END;
        [TestMethod]
        public void TestCompoundStatementMixed()
        {
            List<IToken> tokenList = new()
            {
                // BEGIN
                // MUTABLE TEXT a;
                // a ++ "ala";
                // END
                BeginT(),
                KeywordT("MUTABLE"),
                KeywordT("TEXT"),
                IdentifierT("a"),
                DotT(),
                IdentifierT("a"),
                OperatorT("++"),
                LiteralT("ala"),
                DotT(),
                EndT()
            };
            IInstruction? expected = CompoundS(new()
            {
                VariableD(true, VT.Text, IdentifierE("a"), null),
                BinaryE(IdentifierE("a"), BOT.OperatorConcatenate, LiteralE("ala")),
            });

            var scanner = new MockParserScanner();
            var parser = new Parser(scanner, new MockLogger());
            TestInstructions(scanner, tokenList, expected, parser.BuildCompoundStatement);
        }
    }
}
