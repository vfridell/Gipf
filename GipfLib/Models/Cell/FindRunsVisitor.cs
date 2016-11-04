using System;
using System.Collections.Generic;
using System.Linq;

namespace GipfLib.Models
{
    public class FindRunsVisitor : ICellVisitor
    {
        public IReadOnlyList<IReadOnlyList<Cell>> RunsOfFourOrMore => _runs.AsReadOnly().Where(l => l[0].Piece.Color != PieceColor.None && l.Count >= 4).ToList();
        public IReadOnlyList<IReadOnlyList<Cell>> ExtendedRuns => _extendedRunsOfFour.AsReadOnly();

        private List<List<Cell>> _runs = new List<List<Cell>>();
        private List<List<Cell>> _extendedRunsOfFour = new List<List<Cell>>();
        private List<Cell> _intersections = new List<Cell>();

        public IReadOnlyList<Cell> Intersections => _intersections.AsReadOnly();
        public IReadOnlyList<IReadOnlyList<Cell>> Runs => _runs.AsReadOnly();
        private GipfPiece _lastPiece = new GipfPiece(-1, PieceColor.None);
        private int _index = -1;

        public void VisitCellHandler(object sender, VisitCellEventArgs e)
        {
            if (e.cell.Piece.Color != _lastPiece.Color || e.newLine)
            {
                _runs.Add(new List<Cell>());
                _runs[++_index].Add(e.cell);
            }
            else
            {
                _runs[_index].Add(e.cell);
            }
            _lastPiece = e.cell.Piece;
        }

        public void VisitWallHandler(object sender, VisitWallEventArgs e)
        {
            //do nothing
        }

        public void PostProcessHandler(object sender, EventArgs e)
        {
            _extendedRunsOfFour = new List<List<Cell>>();
            int index = 0;
            foreach (List<Cell> run in RunsOfFourOrMore)
            {
 
                Cell end1 = run.First();
                Cell end2 = run.Last();
                Position direction2 = Neighborhood.GetDirection(end1.hex, end2.hex);
                Position direction1 = Neighborhood.GetDirection(end2.hex, end1.hex);
                Cell currentCell = end1.NeighborhoodCells[direction1];
                _extendedRunsOfFour.Add(new List<Cell>(run));
                while (!(currentCell is Wall) && currentCell.Piece.Color != PieceColor.None)
                {
                    _extendedRunsOfFour[index].Add(currentCell);
                    currentCell = currentCell.NeighborhoodCells[direction1];
                }
                // now go the other way
                currentCell = end2.NeighborhoodCells[direction2];
                while (!(currentCell is Wall) && currentCell.Piece.Color != PieceColor.None)
                {
                    _extendedRunsOfFour[index].Add(currentCell);
                    currentCell = currentCell.NeighborhoodCells[direction2];
                }

                index++;
            }

            // track intersectons
            HashSet<Hex> hexesInRuns = new HashSet<Hex>();
            _extendedRunsOfFour.All(l => l.All((c) =>
                  {
                      if (hexesInRuns.Contains(c.hex)) _intersections.Add(c);
                      else hexesInRuns.Add(c.hex);
                      return true;
                  }
            ));

        }

        public void PreProcessHandler(object sender, EventArgs e)
        {
            _runs?.Clear();
            _extendedRunsOfFour?.Clear();
            _index = -1;
            _lastPiece = new GipfPiece(-1, PieceColor.None);
        }
    }
}
