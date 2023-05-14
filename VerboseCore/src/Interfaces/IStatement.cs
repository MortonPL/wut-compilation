using System.Collections.Generic;

using VerboseCore.Entities;

namespace VerboseCore.Interfaces
{
    public interface IStatement : IInstruction { }

    public interface IStatementEmpty : IStatement
    {
    }

    public interface IStatementCompound : IStatement
    {
        public List<IInstruction> Instructions { get; }
    }

    public interface IBuiltInCompound: IStatementCompound
    {
        public BuiltInType FunType { get; }
    }

    public interface IStatementIf: IStatement
    {
        public IExpression Condition { get; }
        public IStatement OnTrue { get; }
        public IStatement? OnFalse { get; }
    }

    public interface IStatementJump: IStatement
    {
        public JumpType JumpType { get; }
        public IExpression? ReturnValue { get; }
    }

    public interface IStatementWhile: IStatement
    {
        public IExpression Condition { get; }
        public IStatement Body { get; }
    }

    public interface IStatementAnonMatch: IStatement
    {
        public IExpression Expression { get; }
        public IStatementCompound Body { get; }
    }
}
