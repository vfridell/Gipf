using System;
using System.Collections.Generic;
using System.Linq;

namespace GipfLib.Models
{
    public class FindRunsVisitor : ICellVisitor
    {
        public IReadOnlyList<CellRun> RunsOfFourOrMore => _runs.AsReadOnly().Where(r => r.Color != PieceColor.None && r.Count >= 4).ToList();
        public IReadOnlyList<CellRun> ExtendedRuns => _extendedRunsOfFour.AsReadOnly();

        private List<CellRun> _runs = new List<CellRun>();
        private List<CellRun> _extendedRunsOfFour = new List<CellRun>();
        private List<Cell> _intersections = new List<Cell>();

        public IReadOnlyList<Cell> Intersections => _intersections.AsReadOnly();
        public IReadOnlyList<CellRun> Runs => _runs.AsReadOnly();
        private GipfPiece _lastPiece = new GipfPiece(-1, PieceColor.None);
        private List<Cell> _currentRun;

        public void VisitCellHandler(object sender, VisitCellEventArgs e)
        {
            if (e.cell.Piece.Color != _lastPiece.Color || e.newLine)
            {
                if (null == _currentRun)
                {
                    _currentRun = new List<Cell>();
                    _currentRun.Add(e.cell);
                }
                else
                {
                    _runs.Add(new CellRun(_currentRun));
                    _currentRun.Clear();
                    _currentRun.Add(e.cell);
                }
            }
            else
            {
                _currentRun.Add(e.cell);
            }
            _lastPiece = e.cell.Piece;
        }

        public void VisitWallHandler(object sender, VisitWallEventArgs e)
        {
            //do nothing
        }

        public void PostProcessHandler(object sender, EventArgs e)
        {
            _extendedRunsOfFour = new List<CellRun>();
            foreach (CellRun run in RunsOfFourOrMore)
            {
 
                Cell end1 = run.First;
                Cell end2 = run.Last;
                Position direction2 = Neighborhood.GetDirection(end1.hex, end2.hex);
                Position direction1 = Neighborhood.GetDirection(end2.hex, end1.hex);
                Cell currentCell = end1.NeighborhoodCells[direction1];
                List<Cell> cellList = new List<Cell>(run.Run);
                while (!(currentCell is Wall) && currentCell.Piece.Color != PieceColor.None)
                {
                    cellList.Add(currentCell);
                    currentCell = currentCell.NeighborhoodCells[direction1];
                }
                // now go the other way
                currentCell = end2.NeighborhoodCells[direction2];
                while (!(currentCell is Wall) && currentCell.Piece.Color != PieceColor.None)
                {
                    cellList.Add(currentCell);
                    currentCell = currentCell.NeighborhoodCells[direction2];
                }

                _extendedRunsOfFour.Add(new CellRun(cellList));
            }

            // track intersectons
            HashSet<Hex> hexesInRuns = new HashSet<Hex>();
            _extendedRunsOfFour.All(l => l.Run.All((c) =>
                  {
                      if (hexesInRuns.Contains(c.hex)) _intersections.Add(c);
                      else hexesInRuns.Add(c.hex);
                      return true;
                  }
            ));

        }

        public void PreProcessHandler(object sender, EventArgs e)
        {
            _currentRun = null;
            _runs?.Clear();
            _extendedRunsOfFour?.Clear();
            _lastPiece = new GipfPiece(-1, PieceColor.None);
        }
    }
}
