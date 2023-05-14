using VerboseCore.Abstract;
using VerboseCore.Entities;

namespace VerboseCore.Interfaces
{
    public interface IUnit<T>
    {
        public T Type { get; set; }
        public APosition Position { get; set; }
    }

    public interface IToken: IUnit<TokenType>
    {
        public object? Value { get; set; }
    }

    public interface IInstruction : IUnit<InstructionType>
    {
        public void Accept(IVisitor visitor);
    }
}
