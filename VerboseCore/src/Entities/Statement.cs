using System.Collections.Generic;

using VerboseCore.Abstract;
using VerboseCore.Interfaces;

namespace VerboseCore.Entities
{
    public class Statement : Instruction, IStatement
    { 
    }

    public class StatementEmpty : Statement, IStatementEmpty
    {
        public StatementEmpty()
        {
            _type = InstructionType.StatementEmpty;
        }
    }

    public class StatementCompound : Statement, IStatementCompound
    {
        public List<IInstruction> Instructions { get; }

        public StatementCompound(APosition position, List<IInstruction> instructions)
        {
            Position = position;
            _type = InstructionType.StatementCompound;
            Instructions = instructions;
        }

        public StatementCompound(List<IInstruction> instructions)
        {
            Position = new Position();
            _type = InstructionType.StatementCompound;
            Instructions = instructions;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class StatementIf : Statement, IStatementIf
    {
        public IExpression Condition { get; }
        public IStatement OnTrue { get; }
        public IStatement? OnFalse { get; }

        public StatementIf(APosition position, IExpression condition, IStatement onTrue, IStatement? onFalse)
        {
            Position = position;
            _type = InstructionType.StatementIf;
            Condition = condition;
            OnTrue = onTrue;
            OnFalse = onFalse;
        }

        public StatementIf(IExpression condition, IStatement onTrue, IStatement? onFalse)
        {
            Position = new Position();
            _type = InstructionType.StatementIf;
            Condition = condition;
            OnTrue = onTrue;
            OnFalse = onFalse;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class StatementJump : Statement, IStatementJump
    {
        public JumpType JumpType { get; }
        public IExpression? ReturnValue { get; }

        public StatementJump(APosition position, JumpType jumpType, IExpression? returnValue)
        {
            Position = position;
            _type = InstructionType.StatementJump;
            JumpType = jumpType;
            ReturnValue = returnValue;
        }

        public StatementJump(JumpType jumpType, IExpression? returnValue)
        {
            Position = new Position();
            _type = InstructionType.StatementJump;
            JumpType = jumpType;
            ReturnValue = returnValue;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class StatementWhile : Statement, IStatementWhile
    {
        public IExpression Condition { get; }
        public IStatement Body { get; }

        public StatementWhile(APosition position, IExpression condition, IStatement body)
        {
            Position = position;
            _type = InstructionType.StatementWhile;
            Condition = condition;
            Body = body;
        }

        public StatementWhile(IExpression condition, IStatement body)
        {
            Position = new Position();
            _type = InstructionType.StatementWhile;
            Condition = condition;
            Body = body;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class StatementAnonMatch : Statement, IStatementAnonMatch
    {
        public IExpression Expression { get; }
        public IStatementCompound Body { get; }

        public StatementAnonMatch(APosition position, IExpression expression, IStatementCompound body)
        {
            Position = position;
            Expression = expression;
            Body = body;
        }

        public StatementAnonMatch(IExpression expression, IStatementCompound body)
        {
            Position = new Position();
            Expression = expression;
            Body = body;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
