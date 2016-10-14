using System;

namespace GipfLib.Models
{
    public struct Hex
    {
        public static Hex InvalidHex = new Hex(int.MaxValue, int.MinValue);

        public Hex(int column, int row)
        {
            this.column = column;
            this.row = row;

            this.x = column;
            this.z = row;
            this.y = -x - z;
        }

        public Hex(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            this.column = x;
            this.row = z;

            if (x + y + z != 0) throw new Exception(string.Format("Bad Hex cube coordinates: {0}, {1}, {2}", x, y, z));
        }

        public readonly int column;
        public readonly int row;

        public readonly int x;
        public readonly int y;
        public readonly int z;

        public override bool Equals(object obj)
        {
            if (!(obj is Hex)) return false;
            return Equals((Hex)obj);
        }

        public bool Equals(Hex other) => column == other.column && row == other.row;

        public override int GetHashCode() => column * 17 + row;

        public static bool operator !=(Hex hex1, Hex hex2) => !hex1.Equals(hex2);

        public static bool operator ==(Hex hex1, Hex hex2) => hex1.Equals(hex2);

        public static Hex operator +(Hex hex1, Hex hex2) => new Hex(hex1.column + hex2.column, hex1.row + hex2.row);
        public static Hex operator -(Hex hex1, Hex hex2) => new Hex(hex1.column - hex2.column, hex1.row - hex2.row);
        public static Hex operator /(Hex hex, int scalar) => new Hex(hex.column / scalar, hex.row / scalar);

        public static int Distance(Hex hex1, Hex hex2) => 
            Math.Max(Math.Max(Math.Abs(hex1.x - hex2.x), Math.Abs(hex1.y - hex2.y)), Math.Abs(hex1.z - hex2.z));

        public static Hex DirectionHex(Hex fromHex, Hex toHex) => (toHex - fromHex) / Distance(fromHex, toHex);

        public static bool ContainsZeroCoordinate(Hex hex) => hex.x == 0 || hex.y == 0 || hex.z == 0;

        public static bool AxisAligned(Hex hex1, Hex hex2) => ContainsZeroCoordinate(hex1 - hex2);
    }
}
