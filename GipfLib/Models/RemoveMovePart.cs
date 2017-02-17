using System;
using System.Collections.Generic;
using System.Linq;

namespace GipfLib.Models
{
    public class RemoveMovePart
    {
        public RemoveMovePart(IEnumerable<Hex> removeList)
        {
            _hexesToRemove = removeList.ToList();
            _direction = Neighborhood.GetDirection(_hexesToRemove.First(), _hexesToRemove.Last());
        }

        private readonly List<Hex> _hexesToRemove;
        public IReadOnlyList<Hex> HexesToRemove => _hexesToRemove;

        private Position _direction;
        public Position Direction => _direction;
        public Position DirectionOpposite => Neighborhood.GetOpposite(_direction);

        public Hex DirectionHex => Neighborhood.GetDirectionHex(Direction);
        public Hex DirectionOppositeHex => Neighborhood.GetDirectionHex(DirectionOpposite);

        public bool ContiguousAfterRemoval(Hex hex)
        {
            if (!_hexesToRemove.Contains(hex)) throw new Exception($"Given hex ({hex.column}, {hex.row}) is not in this removeMovePart");
            return Hex.IsContiguous(_hexesToRemove.Where(h => h != hex), Direction);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RemoveMovePart)) return false;
            return Equals((RemoveMovePart)obj);
        }

        public bool Equals(RemoveMovePart other)
        {
            return Helpers.ScrambledEquals(_hexesToRemove, other._hexesToRemove);
        }

        public override int GetHashCode() => _hexesToRemove.Sum(hex => hex.GetHashCode());

        public static bool operator !=(RemoveMovePart remove1, RemoveMovePart remove2) => !remove1.Equals(remove2);
        public static bool operator ==(RemoveMovePart remove1, RemoveMovePart remove2) => remove1.Equals(remove2);
    }
}
