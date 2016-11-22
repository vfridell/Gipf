using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GipfLib.Models
{
    public class CellRun
    {
        private readonly List<Cell> _run;
        public IReadOnlyList<Cell> Run => _run;
        public PieceColor Color => _run[0].Piece.Color;
        public int Count => _run.Count;
        public Cell First => _run.First();
        public Cell Last => _run.Last();
        public bool AllGipf { get; }

        public CellRun(IEnumerable<Cell> run)
        {
            _run = new List<Cell>(run);
            AllGipf = _run.All(c => c.Piece.NumPieces == 2);
        }

        public IEnumerable<RemoveMovePart> GetRemoveLists()
        {
            List<RemoveMovePart> returnList = new List<RemoveMovePart>();
            if (Count < 4 || Color == PieceColor.None) return returnList;

            IEnumerable<Hex> basis = _run.Where(c => c.Piece.NumPieces == 1).Select(c => c.hex);
            returnList.Add(new RemoveMovePart(basis));

            foreach (Hex hex in _run.Where(c => c.Piece.NumPieces == 2).Select(c => c.hex))
            {
                List<List<Hex>> tempListList = new List<List<Hex>>();
                foreach (RemoveMovePart removePart in returnList)
                {
                    List<Hex> tempList = new List<Hex>(removePart.HexesToRemove);
                    tempList.Add(hex);
                    tempListList.Add(tempList);
                }
                foreach (List<Hex> tempList in tempListList) returnList.Add(new RemoveMovePart(tempList));
            }
            return returnList;
        }
    }
}
