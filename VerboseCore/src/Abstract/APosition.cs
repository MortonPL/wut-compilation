namespace VerboseCore.Abstract
{
    public abstract class APosition
    {
        protected uint Row { get; set; } = 1;
        protected uint Column { get; set; } = 1;

        public APosition()
        {
            Row = 1;
            Column = 0;
        }

        public APosition(uint row, uint column)
        {
            Row = row;
            Column = column;
        }

        public APosition(APosition other)
        {
            Row = other.Row;
            Column = other.Column;
        }

        public abstract override string ToString();

        public void AddRow()
        {
            Row += 1;
            Column = 1;
        }

        public void AddColumn()
        {
            Column += 1;
        }
    }
}
