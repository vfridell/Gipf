using System.Collections.Generic;
using System.Linq;

namespace GipfLib.Models
{
    public class RemoveMovePart
    {
        public RemoveMovePart()
        {
            _hexesToRemove = new List<Hex>();
        }

        public RemoveMovePart(IEnumerable<Hex> removeList)
        {
            _hexesToRemove = removeList.ToList();
        }

        private readonly List<Hex> _hexesToRemove;
        public IReadOnlyList<Hex> HexesToRemove => _hexesToRemove;

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
