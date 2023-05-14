using System.Collections.Generic;

using VerboseCore.Abstract;
using VerboseCore.Interfaces;

namespace VerboseCore.Entities
{
    public class Expression: Instruction, IExpression { }

    public class ExpressionAssignment: Expression, IExpressionAssignment
    {
        public IExpression From { get; }
        public IExpression To { get; }

        public ExpressionAssignment(APosition position, IExpression from, IExpression to)
        {
            Position = position;
            _type = InstructionType.ExpressionAssignment;
            From = from;
            To = to;
        }

        public ExpressionAssignment(IExpression from, IExpression to)
        {
            Position = new Position();
            _type = InstructionType.ExpressionAssignment;
            From = from;
            To = to;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class ExpressionPipe : Expression, IExpressionPipe
    {
        public IExpression First { get; }
        public IExpression? Then { get; }
        public IExpression? Otherwise { get; }

        public ExpressionPipe(APosition position, IExpression first, IExpression? then, IExpression? otherwise)
        {
            Position = position;
            _type = InstructionType.ExpressionPipe;
            First = first;
            Then = then;
            Otherwise = otherwise;
        }

        public ExpressionPipe(IExpression first, IExpression? then, IExpression? otherwise)
        {
            Position = new Position();
            _type = InstructionType.ExpressionPipe;
            First = first;
            Then = then;
            Otherwise = otherwise;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class ExpressionTernary : Expression, IExpressionTernary
    {
        public IExpression Condition { get; }
        public IExpression OnYes { get; }
        public IExpression? OnNo { get; }

        public ExpressionTernary(APosition position, IExpression condition, IExpression onYes, IExpression? onNo)
        {
            Position = position;
            _type = InstructionType.ExpressionTernary;
            Condition = condition;
            OnYes = onYes;
            OnNo = onNo;
        }

        public ExpressionTernary(IExpression condition, IExpression onYes, IExpression? onNo)
        {
            Position = new Position();
            _type = InstructionType.ExpressionTernary;
            Condition = condition;
            OnYes = onYes;
            OnNo = onNo;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class ExpressionBinary : Expression, IExpressionBinary
    {
        public IExpression Left { get; }
        public BinaryOperatorType Op { get; }
        public IExpression Right { get; }

        public ExpressionBinary(APosition position, IExpression left, BinaryOperatorType op, IExpression right)
        {
            Position = position;
            _type = InstructionType.ExpressionBinary;
            Left = left;
            Op = op;
            Right = right;
        }

        public ExpressionBinary(IExpression left, BinaryOperatorType op, IExpression right)
        {
            Position = new Position();
            _type = InstructionType.ExpressionBinary;
            Left = left;
            Op = op;
            Right = right;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class ExpressionUnary : Expression, IExpressionUnary
    {
        public IExpression Value { get; }
        public UnaryOperatorType Op { get; }

        public ExpressionUnary(APosition position, IExpression value, UnaryOperatorType op)
        {
            Position = position;
            _type = InstructionType.ExpressionUnary;
            Value = value;
            Op = op;
        }

        public ExpressionUnary(IExpression value, UnaryOperatorType op)
        {
            Position = new Position();
            _type = InstructionType.ExpressionUnary;
            Value = value;
            Op = op;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class ExpressionNoneTest : Expression, IExpressionNoneTest
    {
        public IExpression Value { get; }

        public ExpressionNoneTest(APosition position, IExpression value)
        {
            Position = position;
            _type = InstructionType.ExpressionNoneTest;
            Value = value;
        }

        public ExpressionNoneTest(IExpression value)
        {
            Position = new Position();
            _type = InstructionType.ExpressionNoneTest;
            Value = value;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class ExpressionIdentifier : Expression, IExpressionIdentifier
    {
        public string Name { get; }

        public ExpressionIdentifier(APosition position, string name)
        {
            Position = position;
            _type = InstructionType.ExpressionIdentifier;
            Name = name;
        }

        public ExpressionIdentifier(string name)
        {
            Position = new Position();
            _type = InstructionType.ExpressionIdentifier;
            Name = name;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class ExpressionLiteral : Expression, IExpressionLiteral
    {
        public VariableType ValueType { get; }
        public object? Value { get; }

        public ExpressionLiteral(APosition position, object? value, VariableType valueType)
        {
            Position = position;
            _type = InstructionType.ExpressionLiteral;
            Value = value;
            ValueType = valueType;
        }

        public ExpressionLiteral(object? value, VariableType valueType)
        {
            Position = new Position();
            _type = InstructionType.ExpressionLiteral;
            Value = value;
            ValueType = valueType;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class ExpressionCall : Expression, IExpressionCall
    {
        public IExpressionIdentifier Identifier { get; }
        public List<IExpression> Args { get; }

        public ExpressionCall(APosition position, IExpressionIdentifier name, List<IExpression> args)
        {
            Position = position;
            _type = InstructionType.ExpresionCall;
            Identifier = name;
            Args = args;
        }

        public ExpressionCall(IExpressionIdentifier name, List<IExpression> args)
        {
            Position = new Position();
            _type = InstructionType.ExpresionCall;
            Identifier = name;
            Args = args;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
