namespace GipfLib.Models
{
    public static class Pieces
    {
        private static readonly GipfPiece _noPiece = new GipfPiece(0, PieceColor.None);
        private static readonly GipfPiece _whiteGipf = new GipfPiece(2, PieceColor.White);
        private static readonly GipfPiece _blackGipf = new GipfPiece(2, PieceColor.Black);
        private static readonly GipfPiece _white = new GipfPiece(1, PieceColor.White);
        private static readonly GipfPiece _black = new GipfPiece(1, PieceColor.Black);

        public static GipfPiece NoPiece { get { return _noPiece; } }
        public static GipfPiece WhiteGipf { get { return _whiteGipf; } }
        public static GipfPiece BlackGipf { get { return _blackGipf; } }
        public static GipfPiece White { get { return _white; } }
        public static GipfPiece Black { get { return _black; } }

        public static GipfPiece GetPiece(Board board, Move move) =>
            board.WhiteToPlay ? (move.isGipf ? WhiteGipf : White) : (move.isGipf ? BlackGipf : Black);
    }
}
