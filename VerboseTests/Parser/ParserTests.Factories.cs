using System.Collections.Generic;
using System;

using VerboseCore.Entities;
using VerboseCore.Interfaces;
using VerboseCore.Lexer;

using TT = VerboseCore.Entities.TokenType;
using UOT = VerboseCore.Entities.UnaryOperatorType;
using BOT = VerboseCore.Entities.BinaryOperatorType;
using VT = VerboseCore.Entities.VariableType;

namespace VerboseTests.Parser_
{
    public partial class ParserTests
    {
        static public IToken Token() => new Token(TT.Any, new Position());
        static public IToken Token(TT tt) => new Token(tt, new Position());
        static public IToken Token(TT tt, object? value) => new Token(tt, new Position(), value);
        static public IExpression Expression() => new Expression();

        static public IToken LiteralT(object literal) => literal switch
        {
            int => Token(TT.ValueNumber, Convert.ToDouble(literal)),
            double => Token(TT.ValueNumber, Convert.ToDouble(literal)),
            string => Token(TT.ValueText, literal),
            bool => Token(TT.ValueFact, literal),
            _ => Token()
        };
        static public IToken KeywordT(string keyword) => Token(Lexer.MapKeyword(keyword));
        static public IToken IdentifierT(string identifier) => Token(TT.ValueIdentifier, identifier);
        static public IToken OperatorT(string op) => Token(
            op.Length == 3 ? Lexer.MapTripleOp(op[0])!.GetValueOrDefault(op[1])!.GetValueOrDefault(op[2])
            : op.Length == 2 ? Lexer.MapDoubleOp(op[0])!.GetValueOrDefault(op[1])
            : Lexer.MapSignleOp(op[0]));
        static public IToken DotT() => Token(TT.OperatorDot);
        static public IToken BeginT() => Token(TT.KeywordBegin);
        static public IToken EndT() => Token(TT.KeywordEnd);

        static public List<IToken> TrailDot(List<IToken> tokens)
        {
            tokens.Add(DotT());
            return tokens;
        }
        static public List<IToken> Compound(List<IToken> tokens)
        {
            tokens.Insert(0, KeywordT("BEGIN"));
            tokens.Add(KeywordT("END"));
            return tokens;
        }

        static public IExpressionLiteral LiteralE(object? literal) => literal switch
        {
            int => new ExpressionLiteral(Convert.ToDouble(literal), VT.Number),
            double => new ExpressionLiteral(literal, VT.Number),
            string => new ExpressionLiteral(literal, VT.Text),
            bool => new ExpressionLiteral(literal, VT.Fact),
            null => new ExpressionLiteral(literal, VT.Number),
            _ => new ExpressionLiteral(null, VT.Nothing),
        };
        static public IExpressionIdentifier IdentifierE(string name) => new ExpressionIdentifier(name);
        static public IExpressionUnary UnaryE(IExpression target, UOT uot)
            => new ExpressionUnary(target, uot);
        static public IExpressionBinary BinaryE(IExpression left, BOT bot, IExpression right)
            => new ExpressionBinary(left, bot, right);
        static public IExpressionAssignment AssignmentE(IExpression from, IExpression to)
            => new ExpressionAssignment(from, to);
        static public IStatementCompound CompoundS(List<IInstruction> instructions)
            => new StatementCompound(instructions);
        static public IStatementIf IfS(IExpression expression, IStatement onTrue, IStatement? onFalse)
            => new StatementIf(expression, onTrue, onFalse);
        static public IDeclarator Declarator(bool mutable, VT type, IExpressionIdentifier identifier)
            => new Declarator(mutable, type, identifier);
        static public IDeclarationVariable VariableD(bool mutable, VT type, IExpressionIdentifier identifier, IExpression? expression)
            => new DeclarationVariable(new Declarator(mutable, type, identifier), expression);
        static public IDeclarationFunction FunctionD(VT type, IExpressionIdentifier identifier, List<IDeclarator> parameters, IStatement body, bool @override=false)
            => new DeclarationFunction(new Declarator(false, type, identifier), parameters, body, @override);
        static public IDeclarationPattern PatternD(IExpressionIdentifier identifier, IDeclarator parameter, IStatement body, bool @override=false)
            => new DeclarationPattern(new Declarator(false, VT.Nothing, identifier), parameter, body, @override);

    }
}
