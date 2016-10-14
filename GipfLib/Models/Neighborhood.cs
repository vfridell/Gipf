using System;

namespace GipfLib.Models
{
    public enum Position { center, top, topright, bottomright, bottom, bottomleft, topleft };

    // Axial coordinates
    // Flat Topped
    // ref: http://www.redblobgames.com/grids/hexagons/
    class Neighborhood
    {
        public static readonly Hex[] neighborDirections = { new Hex(0, 0), new Hex(0, -1), new Hex(1, -1), new Hex(1, 0), new Hex(0, 1), new Hex(-1, 1), new Hex(-1, 0), };

        public static Hex GetDirectionHex(Position position)
        {
            return neighborDirections[(int)position];
        }

        public static Hex GetNeighborHex(Hex hex, Position position)
        {
            Hex dir = GetDirectionHex(position);
            return new Hex(hex.column + dir.column, hex.row + dir.row);
        }

        public static Position GetOpposite(Position position)
        {
            switch (position)
            {
                case Position.center:
                    return Position.center;
                case Position.topleft:
                    return Position.bottomright;
                case Position.topright:
                    return Position.bottomleft;
                case Position.top:
                    return Position.bottom;
                case Position.bottomright:
                    return Position.topleft;
                case Position.bottomleft:
                    return Position.topright;
                case Position.bottom:
                    return Position.top;
                default:
                    throw new Exception("Invalid position");
            }
        }

        /// <summary>
        /// This will get the "best" direction hex to apply to go from one hex coord to the other.
        /// To ensure axis alignment first, use Hex.AxisAligned()
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>A position enum value for the best direction between the two hexes</returns>
        public static Position GetDirection(Hex from, Hex to)
        {
            // sanity check
            if (from == Hex.InvalidHex || to == Hex.InvalidHex) return Position.center;

            Hex direction = Hex.DirectionHex(from, to);
            for (int i = 0; i < neighborDirections.Length; i++)
            {
                if (neighborDirections[i] == direction) return (Position)i;
            }
            // invalid direction.  I think it will never get here due to integer division rounding down to zero
            return Position.center;
        }

        public static Hex GetOppositeHex(Position position)
        {
            Hex vector = neighborDirections[(int)position];
            return new Hex(-vector.x, -vector.y, -vector.z);
        }

        public static Hex ClockwiseDelta(Position position)
        {
            Hex vector = neighborDirections[(int)position];
            return new Hex(-vector.z, -vector.x, -vector.y);
        }

        public static Hex CounterClockwiseDelta(Position position)
        {
            Hex vector = neighborDirections[(int)position];
            return new Hex(-vector.y, -vector.z, -vector.x);
        }
    }
}
