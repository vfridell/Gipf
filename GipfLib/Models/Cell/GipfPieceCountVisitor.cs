using System;
using System.Collections.Generic;

namespace GipfLib.Models
{
    public class PieceCountVisitor : ICellVisitor
    {
        private HashSet<Hex> _blackGipfs;
        private HashSet<Hex> _whiteGipfs;
        private HashSet<Hex> _whiteNonGipfs;
        private HashSet<Hex> _blackNonGipfs;
        public int TotalPieceCount => (TotalGipfCount * 2) + TotalPieceCount;
        public int TotalNonGipfCount => _blackNonGipfs.Count + _whiteNonGipfs.Count;
        public int TotalGipfCount => _blackGipfs.Count + _whiteGipfs.Count;
        public int WhiteGipfCount => _whiteGipfs.Count;
        public int BlackGipfCount => _blackGipfs.Count;
        public int WhiteNonGipfCount => _whiteNonGipfs.Count;
        public int BlackNonGipfCount => _blackNonGipfs.Count;

        public void PostProcessHandler(object sender, EventArgs e)
        {
            // do nothing
        }

        public void PreProcessHandler(object sender, EventArgs e)
        {
            _blackGipfs = new HashSet<Hex>();
            _whiteGipfs = new HashSet<Hex>();
            _blackNonGipfs = new HashSet<Hex>();
            _whiteNonGipfs = new HashSet<Hex>();
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
            else if (e.cell.Piece.NumPieces == 1)
            {
                if (e.cell.Piece.Color == PieceColor.White)
                    _whiteNonGipfs.Add(e.cell.hex);
                else
                    _blackNonGipfs.Add(e.cell.hex);
            }
        }

        public void VisitWallHandler(object sender, VisitWallEventArgs e)
        {
            // do nothing
        }
    }
}
