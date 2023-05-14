using VerboseCore.Abstract;

namespace VerboseCore.Entities
{
    public class Position: APosition
    {
        public Position() : base() { }
        public Position(uint row, uint column) : base(row, column) { }
        public Position(Position other) : base(other) { }
        public Position(APosition other) : base(other) { }

        public override string ToString() => $"({Row}, {Column})";
    }
}
