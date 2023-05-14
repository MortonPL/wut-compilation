using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using VerboseCore.Abstract;
using VerboseCore.Entities;
using VerboseCore.Exceptions;
using VerboseCore.Interfaces;

[assembly: InternalsVisibleTo("VerboseTests")]
namespace VerboseCore.Parser
{
    public partial class Parser: IParser
    {
        private readonly ILogger _logger;
        private readonly IParserScanner _scanner;
        private IInstruction _instruction;

        public string BufferedError { get => _scanner.BufferedError; }

        public Parser(IParserScanner scanner, ILogger logger)
        {
            _scanner = scanner;
            _logger = logger;
            _instruction = new Instruction();
        }

        public Parser(Stream stream, ILogger logger)
        {
            _scanner = new ParserScanner(new Lexer.Lexer(stream, logger));
            _logger = logger;
            _instruction = new Instruction();
        }

        // program = {declaration | statement};
        public IInstruction BuildProgram()
        {
            while (_scanner.Type != TokenType.SpecialETX)
            {
                try
                {
                    return _instruction = BuildCompoundStatementNaked()!;
                }
                catch(LexerError e)
                {
                    throw e;
                }
                catch (ParserError e)
                {
                    _logger.EmitError(e.Error, _scanner.Position, BufferedError, e.Values);
                    throw e;
                }
            }

            return _instruction;
        }

        /************************** STATEMENT BUILDERS **************************/

        internal IStatement? BuildStatement()
        {
            return BuildCompoundStatement()
                   ?? BuildExpressionStatement()
                   ?? BuildIfStatement()
                   ?? BuildIterationStatement()
                   ?? BuildJumpStatement()
                   ?? BuildAnonMatchStatement()
                   ?? BuildEmptyStatement();
        }

        // compound_statement = BEGIN, {declaration | statement}, END;
        internal IStatement? BuildCompoundStatement()
        {
            // BEGIN
            if(!TryExpectToken(TokenType.KeywordBegin)) return null;

            var compound = BuildCompoundStatementNaked();

            // END
            ExpectToken(TokenType.KeywordEnd);

            return compound;
        }

        IStatement? BuildCompoundStatementNaked()
        {
            var pos = _scanner.Position;
            // {declaration | statement}
            IStatement? statement = null;
            IDeclaration? declaration;
            List<IInstruction> instructions = new();
            while (((declaration = BuildDeclaration()) != null)
                || ((statement = BuildStatement()) != null))
            {
                if (declaration != null) instructions.Add(declaration);
                else if (statement != null) instructions.Add(statement);
            }

            return new StatementCompound(pos, instructions);
        }

        // expression_statement = expression, DOT;
        internal IStatement? BuildExpressionStatement()
        {
            // expression
            IExpression? expression;
            if ((expression = BuildExpression()) == null) return null;
            
            // DOT
            ExpectToken(TokenType.OperatorDot);

            return expression;
        }

        // if_statement = IF, expression, DO, statement, [ELSE, statement];
        internal IStatement? BuildIfStatement()
        {
            var pos = _scanner.Position;
            // IF
            if (!TryExpectToken(TokenType.KeywordIf)) return null;

            // expression
            var condition = ExpectNotNull(BuildExpression(), SymbolType.Expression);

            // DO
            ExpectToken(TokenType.KeywordDo);

            // statement
            IStatement onTrue = ExpectNotNull(BuildStatement(), SymbolType.Statement);
            IStatement? onFalse = null;

            // [ELSE, statement]
            ZeroOrOnce(TokenType.KeywordElse, () =>
            {
                onFalse = ExpectNotNull(BuildStatement(), SymbolType.Statement);
            });

            return new StatementIf(pos, condition, onTrue, onFalse);
        }

        // jump_statement = SKIP | STOP | (RETURN, [expression]), DOT;
        internal IStatement? BuildJumpStatement()
        {
            var pos = _scanner.Position;
            // SKIP | STOP || (RETURN ...)
            JumpType type = JumpType.Any;
            IStatement? statement = null;
            ZeroOrOnce(new List<TokenType>()
                {
                TokenType.KeywordSkip,
                TokenType.KeywordStop,
                TokenType.KeywordReturn
                }, ()=>
            {
                type = MapJumpType(_scanner.Type);
            }, () =>
            {
                IExpression? expression = null;
                // (RETURN, [expression])
                if (type == JumpType.Return)
                    expression = BuildExpression();

                // DOT
                ExpectToken(TokenType.OperatorDot);
                statement = new StatementJump(pos, type, expression);
            });

            return statement;
        }

        // iteration_statement = for_statement | while_statement;
        internal IStatement? BuildIterationStatement() => BuildForStatement() ?? BuildWhileStatement();

        // returns a compound statement instead of a separate type
        // for_statement = FOR, variable_declaration, while_statement;
        internal IStatement? BuildForStatement()
        {
            var pos = _scanner.Position;
            // WHILE
            if (!TryExpectToken(TokenType.KeywordFor)) return null;

            // variable_declaration
            var declaration = ExpectNotNull(BuildVariableDeclaration(), SymbolType.DeclarationVariable);

            // while_statement
            var statement = ExpectNotNull(BuildWhileStatement(), SymbolType.StatementWhile);

            return new StatementCompound(pos, new(){declaration, statement});
        }

        // while_statement = WHILE, expression, DO, statement;
        internal IStatement? BuildWhileStatement()
        {
            var pos = _scanner.Position;
            // WHILE
            if (!TryExpectToken(TokenType.KeywordWhile)) return null;

            // expression
            var expression = ExpectNotNull(BuildExpression(), SymbolType.Expression);

            // DO
            ExpectToken(TokenType.KeywordDo);

            // statement
            var statement = ExpectNotNull(BuildStatement(), SymbolType.Statement);

            return new StatementWhile(pos, expression, statement);
        }

        // anon_match = MATCH, WITH, expression, match_block;
        internal IStatement? BuildAnonMatchStatement()
        {
            var pos = _scanner.Position;
            // MATCH
            if (!TryExpectToken(TokenType.KeywordMatch)) return null;

            // WITH
            ExpectToken(TokenType.KeywordWith);

            // expression
            var expression = ExpectNotNull(BuildExpression(), SymbolType.Expression);

            // match_block
            var body = ExpectNotNull(BuildMatchBlock(), SymbolType.MatchBlock);

            return new StatementAnonMatch(pos, expression, body);
        }

        // DOT
        IStatement? BuildEmptyStatement()
        {
            var pos = _scanner.Position;
            // DOT
            if (!TryExpectToken(TokenType.OperatorDot)) return null;

            var st = new StatementEmpty{ Position = pos };
            return st;
        }

        // note: the branches are just ifs
        // match_block = BEGIN, {match_branch}, DEFAULT, statement, COMMA, END;
        IStatementCompound? BuildMatchBlock()
        {
            // BEGIN
            if (!TryExpectToken(TokenType.KeywordBegin)) return null;
            var start = _scanner.Position;

            // {match_branch}
            IStatementIf? branch;
            List<IInstruction> branches = new();
            while ((branch = BuildMatchBranch()) != null)
                branches.Add(branch!);

            // DEFAULT
            ExpectToken(TokenType.KeywordDefault);

            var pos = _scanner.Position;
            // statement
            IExpression def = new ExpressionLiteral(pos, true, VariableType.Fact);
            IStatement statement = ExpectNotNull(BuildStatement(), SymbolType.Statement);
            branches.Add(new StatementIf(pos, def, statement, null));

            // COMMA
            ExpectToken(TokenType.OperatorComma);

            // END
            ExpectToken(TokenType.KeywordEnd);

            return new StatementCompound(start, branches);
        }

        // match_branch = expression, DO, statement, COMMA;
        internal IStatementIf? BuildMatchBranch()
        {
            var pos = _scanner.Position;
            IExpression? expression;
            // expression
            if ((expression = BuildExpression()) == null) return null;

            // DO
            ExpectToken(TokenType.KeywordDo);

            // statement
            var statement = ExpectNotNull(BuildStatement(), SymbolType.Statement);

            // COMMA
            ExpectToken(TokenType.OperatorComma);

            return new StatementIf(pos, expression, statement, null);
        }

        /************************** DECLARATION BUILDERS **************************/

        // declaration = (variable_declaration, DOT) | function_declaration | pattern_declaration;
        internal IDeclaration? BuildDeclaration()
        {
            // (variable_declaration, DOT) | 
            IDeclaration? declaration;
            if((declaration = BuildVariableDeclaration()) != null)
            {
                // DOT
                ExpectToken(TokenType.OperatorDot);
            }

            // function_declaration |
            else if ((declaration = BuildFunctionDeclaration()) != null)
            {
            }

            // pattern_declaration
            else if ((declaration = BuildPatternDeclaration()) != null)
            {
            }

            else
            {
                return null;
            }

            return declaration;
        }

        // variable_declaration = declarator, [IS, expression];
        internal IDeclaration? BuildVariableDeclaration()
        {
            var pos = _scanner.Position;
            // declarator
            IDeclarator? declarator;
            if((declarator = BuildDeclarator()) == null) return null;

            // [IS, expression]
            IExpression? expression = null;
            ZeroOrOnce(TokenType.KeywordIs, () =>
            {
                expression = ExpectNotNull(BuildExpression(), SymbolType.Expression);
            });

            return new DeclarationVariable(pos, declarator, expression);
        }

        // function_declaration = FUNCTION, [OVERRIDE], IDENTIFIER, [WITH, parameters], RETURNS, return_type, statement;
        // parameters = declarator, {COMMA, declarator}
        internal IDeclaration? BuildFunctionDeclaration()
        {
            var pos = _scanner.Position;
            var @override = false;

            // FUNCTION
            if (!TryExpectToken(TokenType.KeywordFunction)) return null;

            // [OVERRIDE]
            ZeroOrOnce(TokenType.KeywordOverride, () =>
            {
                @override = true;
            });

            // IDENTIFIER
            var identifier = ExpectNotNull(BuildIdentifier(), SymbolType.ExpressionIdentifier);

            // [WITH, parameters]
            // parameters = declarator, {COMMA, declarator}
            List<IDeclarator> parameters = new();
            List<string> parameterNames = new();
            ZeroOrOnce(TokenType.KeywordWith, () =>
            {
                var d = ExpectNotNull(BuildDeclarator(), SymbolType.Declarator);
                parameters.Add(d);
                parameterNames.Add(d.Identifier.Name);
                ZeroOrMany(TokenType.OperatorComma, () =>
                {
                    var d = ExpectNotNull(BuildDeclarator(), SymbolType.Declarator);
                    if (parameterNames.Contains(d.Identifier.Name))
                        throw new ParserError(ErrorType.DuplicateParam, new() { d.Identifier.Name, identifier.Name });
                    parameters.Add(d);
                });
            });

            // RETURNS
            ExpectToken(TokenType.KeywordReturns);

            // return_type
            var ret = ExpectNotNull(BuildReturnType(), SymbolType.ReturnType);

            // statement
            var statement = ExpectNotNull(BuildStatement(), SymbolType.Statement);

            return new DeclarationFunction(pos, new Declarator(pos, false, ret, identifier), parameters, statement, @override);
        }

        // pattern_declaration = PATTERN, [OVERRIDE], IDENTIFIER, WITH, declarator, (match_block | DOT);
        internal IDeclaration? BuildPatternDeclaration()
        {
            var pos = _scanner.Position;
            // PATERN
            if (!TryExpectToken(TokenType.KeywordPattern)) return null;

            var @override = false;
            // [OVERRIDE]
            ZeroOrOnce(TokenType.KeywordOverride, () =>
            {
                @override = true;
            });

            // IDENTIFIER
            var identifier = ExpectNotNull(BuildIdentifier(), SymbolType.ExpressionIdentifier);

            // WITH
            ExpectToken(TokenType.KeywordWith);

            // declarator
            var parameter = ExpectNotNull(BuildDeclarator(), SymbolType.Declarator);

            // (match_block | DOT)
            IStatement? body = null;
            if ((body = BuildMatchBlock()) == null)
            {
                ExpectToken(TokenType.OperatorDot);
                body = new StatementEmpty();
            }

            return new DeclarationPattern(pos, new Declarator(pos, false, VariableType.Nothing, identifier), parameter, body, @override);
        }

        // declarator = [MUTABLE], type, IDENTIFIER;
        IDeclarator? BuildDeclarator()
        {
            var pos = _scanner.Position;
            var parsed = false;

            // [MUTABLE]
            var mutable = false;
            ZeroOrOnce(TokenType.KeywordMutable, () =>
            {
                mutable = true;
                parsed = true;
            });

            // type
            VariableType? type;
            if ((type = TryBuildType()) == null)
                if (parsed)
                    throw new ParserError(ErrorType.ExpectedToken,
                        new (){ SymbolType.KeywordNumber, SymbolType.KeywordText, SymbolType.KeywordFact });
                else
                    return null;

            // IDENTIFIER
            IExpressionIdentifier identifier = ExpectNotNull(BuildIdentifier(), SymbolType.ExpressionIdentifier);

            return new Declarator(pos, mutable, (VariableType)type!, identifier);
        }


        /************************** EXPRESSION BUILDERS **************************/

        // expression = operator_12;
        internal IExpression? BuildExpression() => BuildAssignmentExpression();

        // grammar: operator_12 = {operator_11, IS}, operator_11;
        // note: in implementation, it is actually operator_11, {IS, operator_11},
        // then we build in reverse direction
        internal IExpression? BuildAssignmentExpression()
        {
            // operator_11
            IExpression? first;
            if ((first = BuildPipeExpression()) == null) return null;

            List<IExpression> exprs = new() { first };

            // {IS, operator_11}
            ZeroOrMany(TokenType.KeywordIs, () =>
            {
                exprs.Add(ExpectNotNull(BuildPipeExpression(), SymbolType.Expression));
            });

            IExpression? assignment = null;
            for (int i = exprs.Count - 1; i >= 1; i--)
            {
                assignment = new ExpressionAssignment(exprs[i].Position, assignment ?? exprs[i], exprs[i - 1]);
            }

            return exprs.Count == 1 ? first : assignment;
        }

        // operator_11 = operator_10, {[THEN, operator_10], [OTHERWISE, operator_10]};
        internal IExpression? BuildPipeExpression()
        {
            // operator_10
            IExpression? first;
            if ((first = BuildTernaryExpression()) == null)
                return null;
            
            // {...}
            IExpression? then = null;
            IExpression? otherwise = null;
            TokenType type = TokenType.Any;
            TokenType prevType = TokenType.Any;
            ZeroOrMany(new List<TokenType>()
            { 
                TokenType.KeywordThen, 
                TokenType.KeywordOtherwise
            }, () => { prevType = type = _scanner.Type; },
            () =>
            {
                otherwise = then = null;
                // [THEN, operator_10]
                if (type == TokenType.KeywordThen)
                {
                    then = ExpectNotNull(BuildTernaryExpression(), SymbolType.Expression);
                    type = _scanner.Type;
                }

                // [OTHERWISE, operator_10]
                if (type == TokenType.KeywordOtherwise)
                {
                    if (prevType == TokenType.KeywordThen) _scanner.Next(); // ugly hack :(
                    otherwise = ExpectNotNull(BuildTernaryExpression(), SymbolType.Expression);
                }

                first = new ExpressionPipe(first.Position, first, then, otherwise);
            });
            return first;
        }

        // operator_10 = operator_9, {TERNARY_YES, operator_9, [TERNARY_NO, operator_9]};
        internal IExpression? BuildTernaryExpression()
        {
            // operator_9
            IExpression? first;
            if ((first = BuildBinaryExpressionOr()) == null)
                return null;

            // {TERNARY_YES, operator_9, ...}
            IExpression? ternaryYes = null;
            IExpression? ternaryNo = null;
            ZeroOrMany(TokenType.OperatorTernaryYes, () =>
            {
                ternaryYes = ExpectNotNull(BuildBinaryExpressionOr(), SymbolType.Expression);

                // [TERNARY_NO, operator_9]
                ZeroOrOnce(TokenType.OperatorTernaryNo, () =>
                {
                    ternaryNo = ExpectNotNull(BuildBinaryExpressionOr(), SymbolType.Expression);
                });

                first = new ExpressionTernary(first.Position, first, ternaryYes!, ternaryNo);
            });

            return first;
        }

        // pseudo-production: left, {"operator", right}
        IExpression? BuildBinaryExpression(Func<IExpression?> subexpr, List<TokenType> ops)
        {
            // left
            IExpression? first;
            if ((first = subexpr()) == null)
                return null;

            // {"operator", right}
            BinaryOperatorType op = BinaryOperatorType.Any;
            IExpression? second = null;
            ZeroOrMany(ops, () =>
            {
                op = MapBinary(_scanner.Type);
            }, () =>
            {
                second = ExpectNotNull(subexpr(), SymbolType.Expression); 
                first = new ExpressionBinary(first.Position, first, op, second!);
            });

            return first;
        }

        //operator_9 = operator_8, {OR, operator_8};
        internal IExpression? BuildBinaryExpressionOr() =>
            BuildBinaryExpression(() => BuildBinaryExpressionAnd(), new() { TokenType.KeywordOr });

        // operator_8 = operator_7, {AND, operator_7};
        internal IExpression? BuildBinaryExpressionAnd() =>
            BuildBinaryExpression(() => BuildBinaryExpressionEqual(), new() { TokenType.KeywordAnd });

        // operator_7 = operator_6, {(EQUAL | NOT_EQUAL | EQUAL_TEXT | NOT_EQUAL_TEXT), operator_6};
        internal IExpression? BuildBinaryExpressionEqual() =>
            BuildBinaryExpression(() => BuildBinaryExpressionGreater(), new()
            {
                TokenType.ComparatorEqual,
                TokenType.ComparatorNotEqual,
                TokenType.ComparatorEqualText,
                TokenType.ComparatorNotEqualText,
            });

        // operator_6 = operator_5, { (GREATER | GREATER_EQUAL | LESSER | LESSER_EQUAL), operator_5};
        internal IExpression? BuildBinaryExpressionGreater() =>
            BuildBinaryExpression(() => BuildBinaryExpressionAddition(), new()
            {
                TokenType.ComparatorGreater,
                TokenType.ComparatorGreaterEqual,
                TokenType.ComparatorLess,
                TokenType.ComparatorLessEqual,
            });

        // operator_5 = operator_4, { (PLUS | MINUS | CONCAT), operator_4};
        internal IExpression? BuildBinaryExpressionAddition() =>
            BuildBinaryExpression(() => BuildBinaryExpressionMultiplication(), new()
            {
                TokenType.ArithmeticAdd,
                TokenType.ArithmeticSub,
                TokenType.OperatorConcatenate,
            });

        // operator_4 = operator_3, { (MULTIPLY | DIVIDE | MODULO), operator_3};
        internal IExpression? BuildBinaryExpressionMultiplication() =>
            BuildBinaryExpression(() => BuildUnaryExpression(), new()
            {
                TokenType.ArithmeticMul,
                TokenType.ArithmeticDiv,
                TokenType.ArithmeticMod,
            });

        // operator_3 = [MINUS | NOT], operator_2;
        internal IExpression? BuildUnaryExpression()
        {
            var pos = _scanner.Position;
            var parsed = false;

            // [MINUS | NOT]
            var op = UnaryOperatorType.Any;
            ZeroOrOnce(new List<TokenType>()
                {
                    TokenType.ArithmeticSub,
                    TokenType.KeywordNot
                }, () =>
            {
                op = MapUnary(_scanner.Type);
            }, () =>
            {
                parsed = true;
            });

            // operator_2
            IExpression? expression;
            if ((expression = BuildNoneTestExpression()) == null)
                if (parsed)
                    throw new ParserError(ErrorType.ExpectedSymbol, new() { SymbolType.Expression });
                else
                    return null;

            return parsed ? new ExpressionUnary(pos, expression, op) : expression;
        }

        // operator_2 = (LITERAL | IDENTIFIER | operator_1 | PIPE | VALUE | (PARENTHESIS_OPEN, expression, PARENTHESIS_CLOSE)), [TEST_NONE];
        internal IExpression? BuildNoneTestExpression()
        {
            var pos = _scanner.Position;

            // (LITERAL |
            IExpression? expression;
            if ((expression = TryBuildLiteral()) != null)
            {
            }

            // IDENTIFIER | PIPE | VALUE |
            else if (_scanner.Buffer.Type == TokenType.ValueIdentifier)
            {
                expression = new ExpressionIdentifier(pos, (string)_scanner.Value!);
                _scanner.Next();
            }

            // (PARENTHESIS_OPEN, expression, PARENTHESIS_CLOSE) ) |
            else if (TryExpectToken(TokenType.OperatorParenthesisOpen))
            {
                expression = ExpectNotNull(BuildExpression(), SymbolType.Expression);
                ExpectToken(TokenType.OperatorParenthesisClose);
            }

            // operator_1
            else if ((expression = BuildCallExpression()) != null)
            {
            }

            else
            {
                return null;
            }

            // [TEST_NONE]
            bool parsed = false;
            ZeroOrOnce(new List<TokenType>()
            {
                TokenType.OperatorNoneTest,
            }, () =>
            {
                parsed = true;
            });

            return parsed ? new ExpressionNoneTest(pos, expression) : expression;
        }

        // operator_1 = CALL, IDENTIFIER, [WITH, arguments], NOW;
        internal IExpression? BuildCallExpression()
        {
            var pos = _scanner.Position;
            // CALL
            if (!TryExpectToken(TokenType.KeywordCall)) return null;

            // IDENTIFIER
            var identifier = BuildIdentifier();

            List<IExpression> args = new();
            // [WITH, arguments]
            // arguments  = expression, {COMMA, expression};
            ZeroOrOnce(TokenType.KeywordWith, () =>
            {
                args.Add(ExpectNotNull(BuildExpression(), SymbolType.Expression));
                ZeroOrMany(TokenType.OperatorComma, () =>
                {
                    args.Add(ExpectNotNull(BuildExpression(), SymbolType.Expression));
                });
            });

            // NOW
            ExpectToken(TokenType.KeywordNow);

            return new ExpressionCall(pos, identifier, args);
        }

        /************************** OTHER BUILDERS **************************/

        // return_type = type | NOTHING;
        // type = NUMBER | TEXT | FACT;
        VariableType BuildReturnType()
        {
            var retType = VariableType.Any;
            Once(new List<TokenType>()
                {
                    TokenType.KeywordNumber,
                    TokenType.KeywordText,
                    TokenType.KeywordFact,
                    TokenType.KeywordNothing,
                }, () =>
                {
                    retType = MapVariableType(_scanner.Type);
                }, () =>
                {
                });
            return retType;
        }

        // type = NUMBER | TEXT | FACT;
        // can fail
        VariableType? TryBuildType()
        {
            VariableType? varType = null;
            ZeroOrOnce(new List<TokenType>()
                {
                    TokenType.KeywordNumber,
                    TokenType.KeywordText,
                    TokenType.KeywordFact,
                }, () =>
            {
                varType = MapVariableType(_scanner.Buffer.Type);
            }, () =>
            {
            });
            return varType;
        }

        // LITERAL = number_literal | text_literal | fact_literal | none_literal;
        // can fail
        IExpressionLiteral TryBuildLiteral()
        {
            var pos = _scanner.Position;
            IExpressionLiteral? expression = _scanner.Type switch
            {
                TokenType.ValueNumber => new ExpressionLiteral(pos, _scanner.Value!, VariableType.Number),
                TokenType.ValueText => new ExpressionLiteral(pos, _scanner.Value!, VariableType.Text),
                TokenType.ValueFact => new ExpressionLiteral(pos, _scanner.Value!, VariableType.Fact),
                TokenType.KeywordNone => new ExpressionLiteral(pos, null, VariableType.Number),
                _ => null
            };

            if (expression != null)
                _scanner.Next();

            return expression!;
        }

        IExpressionIdentifier BuildIdentifier()
        {
            var pos = _scanner.Position;
            IExpressionIdentifier? expression = null;
            Once(TokenType.ValueIdentifier, () =>
            { 
                expression = new ExpressionIdentifier(pos, (string)_scanner.Value!);
            }, () => { });
            return expression!;
        }
    }
}
