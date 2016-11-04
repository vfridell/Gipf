using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GipfLib.Models
{
    public class RemoveMovePart
    {
        public RemoveMovePart()
        {
            _hexesToRemove = new List<Hex>();
        }

        public RemoveMovePart(List<Hex> removeList)
        {
            _hexesToRemove = removeList;
        }

        private List<Hex> _hexesToRemove;
        public IReadOnlyList<Hex> hexesToRemove => _hexesToRemove;

        public override bool Equals(object obj)
        {
            if (!(obj is RemoveMovePart)) return false;
            return Equals((RemoveMovePart)obj);
        }

        public bool Equals(RemoveMovePart other)
        {
            return Helpers.ScrambledEquals(_hexesToRemove, other._hexesToRemove);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (Hex hex in _hexesToRemove) hashCode += hex.GetHashCode();
            return hashCode;
        }

        public static bool operator !=(RemoveMovePart remove1, RemoveMovePart remove2) => !remove1.Equals(remove2);
        public static bool operator ==(RemoveMovePart remove1, RemoveMovePart remove2) => remove1.Equals(remove2);



    }
}
