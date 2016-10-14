using System;

namespace GipfLib.Models
{
    public class Wall : Cell
    {
        public override GipfPiece Piece
        {
            get
            {
                return Pieces.NoPiece;
            }
        }

        internal Wall(Board board, Hex hex) : base(board, hex, Pieces.NoPiece) { }

        protected override void PushRecursive(Position position, GipfPiece incomingPiece)
        {
            throw new Exception("Can't push into a Wall");
        }

        public override bool CanPush(Position position)
        {
            if (neighborhood[position] == null) return false;
            return neighborhood[position].CanPush(position);
        }
    }
}
