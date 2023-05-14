using System.Collections.Generic;

using VerboseCore.Entities;

namespace VerboseCore.Interfaces
{
    public interface IExpression: IStatement { }

    public interface IExpressionAssignment: IExpression
    {
        public IExpression From { get; }
        public IExpression To { get; }
    }

    public interface IExpressionPipe: IExpression
    {
        public IExpression First { get; }
        public IExpression? Then { get; }
        public IExpression? Otherwise { get; }
    }

    public interface IExpressionTernary: IExpression
    {
        public IExpression Condition { get; }
        public IExpression OnYes { get; }
        public IExpression? OnNo { get; }
    }

    public interface IExpressionBinary: IExpression
    {
        public IExpression Left { get; }
        public BinaryOperatorType Op { get; }
        public IExpression Right { get; }
    }

    public interface IExpressionUnary: IExpression
    {
        public IExpression Value { get; }
        public UnaryOperatorType Op { get; }
    }

    public interface IExpressionNoneTest : IExpression
    {
        public IExpression Value { get; }
    }

    public interface IExpressionIdentifier : IExpression
    {
        public string Name { get; }
    }

    public interface IExpressionLiteral: IExpression
    {
        public VariableType ValueType { get; }
        public object? Value { get; }
    }

    public interface IExpressionCall: IExpression
    {
        public IExpressionIdentifier Identifier { get; }
        public List<IExpression> Args { get; }
    }
}
