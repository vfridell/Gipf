namespace GipfLib.Models
{
    public enum PieceColor { White, Black, None }

    public struct GipfPiece
    {
        public GipfPiece(int numPieces, PieceColor color) { NumPieces = numPieces; Color = color; }
        public int NumPieces { get; private set; }
        public PieceColor Color { get; private set; }

        public override bool Equals(object obj)
        {
            if (!(obj is GipfPiece)) return false;
            return Equals((GipfPiece)obj);
        }

        public bool Equals(GipfPiece other) => Color == other.Color && NumPieces == other.NumPieces;

        public override int GetHashCode() => (NumPieces + 1) * 17 + (int)Color;

        public static bool operator !=(GipfPiece piece1, GipfPiece piece2) => piece1.Equals(piece2);

        public static bool operator ==(GipfPiece piece1, GipfPiece piece2) => !piece1.Equals(piece2);

    }
}

