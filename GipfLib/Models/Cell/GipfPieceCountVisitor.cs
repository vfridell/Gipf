using System;
using System.Collections.Generic;

namespace GipfLib.Models
{
    public class GipfPieceCountVisitor : ICellVisitor
    {
        private HashSet<Hex> _blackGipfs;
        private HashSet<Hex> _whiteGipfs;
        public int TotalGipfCount => _blackGipfs.Count + _whiteGipfs.Count;
        public int WhiteGipfCount => _whiteGipfs.Count;
        public int BlackGipfCount => _blackGipfs.Count;

        public void PostProcessHandler(object sender, EventArgs e)
        {
            // do nothing
        }

        public void PreProcessHandler(object sender, EventArgs e)
        {
            _blackGipfs = new HashSet<Hex>();
            _whiteGipfs = new HashSet<Hex>();
        }

        public void VisitCellHandler(object sender, VisitCellEventArgs e)
        {
            if (e.cell.Piece.NumPieces == 2)
            {
                if (e.cell.Piece.Color == PieceColor.White)
                    _whiteGipfs.Add(e.cell.hex);
                else
                    _blackGipfs.Add(e.cell.hex);
            }
        }

        public void VisitWallHandler(object sender, VisitWallEventArgs e)
        {
            // do nothing
        }
    }
}
