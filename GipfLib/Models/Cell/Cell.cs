using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GipfLib.Models
{
    public class Cell
    {
        internal Cell(Board board, Hex hex) : this(board, hex, Pieces.NoPiece) { }
        internal Cell(Board board, Hex hex, GipfPiece piece) { _board = board; _hex = hex; Piece = piece; }

        protected Dictionary<Position, Cell> neighborhood = new Dictionary<Position, Cell>();

        protected Hex _hex;
        public Hex hex => _hex;

        public IReadOnlyDictionary<Position, Cell> NeighborhoodCells { get { return new ReadOnlyDictionary<Position, Cell>(neighborhood); } }
        public virtual GipfPiece Piece { get; protected set; }

        protected Board _board;
        public Board board => _board;

        public virtual void SetPiece(GipfPiece piece)
        {
            Piece = piece;
        }

        public static void LinkCells(Cell from, Cell to, Position pos)
        {
            from.neighborhood[pos] = to;
            to.neighborhood[Neighborhood.GetOpposite(pos)] = from;
        }

        public bool TryGetNeighbor(Position position, out Cell cell)
        {
            return neighborhood.TryGetValue(position, out cell);
        }

        public void Push(Position position, GipfPiece incomingPiece)
        {
            if (!CanPush(position)) throw new Exception("Can't push here");
            neighborhood[position].PushRecursive(position, incomingPiece);
        }

        protected virtual void PushRecursive(Position position, GipfPiece incomingPiece)
        {
            GipfPiece oldPiece = Piece;
            Piece = incomingPiece;
            if (oldPiece.Color != PieceColor.None)
            {
                neighborhood[position].PushRecursive(position, oldPiece);
            }
        }

        public virtual bool CanPush(Position position)
        {
            if (Piece.Color == PieceColor.None) return true;
            return neighborhood[position].CanPush(position);
        }

        public override int GetHashCode()
        {
            return _hex.GetHashCode() + Piece.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return Equals((Cell)obj);
        }

        public virtual bool Equals(Cell obj)
        {
            if (obj == null) return false;
            return this.Piece == obj.Piece && this._hex == obj._hex;
        }
    }
}