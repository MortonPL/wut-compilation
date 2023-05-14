using VerboseCore.Abstract;
using VerboseCore.Interfaces;

namespace VerboseCore.Entities
{
    public class Instruction : IInstruction
    {
        protected InstructionType _type;
        protected APosition _position;

        public InstructionType Type { get => _type; set => _type = value; }
        public APosition Position { get => _position; set => _position = value; }

        public Instruction()
        {
            _position = new Position();
        }

        public Instruction(APosition position)
        {
            _position = position;
        }

        public Instruction(APosition position, InstructionType type)
        {
            _type = type;
            _position = position;
        }

        public void Accept(IVisitor visitor) =>
            visitor.Visit(this);
    }
}
