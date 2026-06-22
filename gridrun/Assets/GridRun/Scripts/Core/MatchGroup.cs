using System.Collections.Generic;
using System.Linq;

namespace GridRun.Core
{
    public sealed class MatchGroup
    {
        public MatchGroup(NodeKind kind, IEnumerable<BoardPosition> cells)
        {
            Kind = kind;
            Cells = cells.Distinct().ToList();
            bool sameRow = Cells.Count > 0 && Cells.All(cell => cell.Y == Cells[0].Y);
            bool sameColumn = Cells.Count > 0 && Cells.All(cell => cell.X == Cells[0].X);
            IsStraight = sameRow || sameColumn;
            Axis = sameRow ? PowerUpAxis.Row : sameColumn ? PowerUpAxis.Column : PowerUpAxis.None;
        }

        public NodeKind Kind { get; }
        public List<BoardPosition> Cells { get; }
        public bool IsStraight { get; }
        public PowerUpAxis Axis { get; }
    }
}
