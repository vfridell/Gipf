using System;
using System.Collections.Generic;
using System.Linq;

namespace GipfLib.Models
{
    public class Move
    {
        private string _notation;
        public string notation => _notation;

        bool _isGipf;
        Hex _from;
        Hex _to;
        List<Hex> _removeBefore;
        List<Hex> _removeAfter;

        public bool isGipf => _isGipf;
        public bool isPlacement => _from == Hex.InvalidHex;
        public Hex from => _from;
        public Hex to => _to;
        public IReadOnlyList<Hex> removeBefore => _removeBefore?.AsReadOnly();
        public IReadOnlyList<Hex> removeAfter => _removeAfter?.AsReadOnly();

        public static Move GetMove(string notation)
        {
            Move move;
            if (NotationParser.TryParseNotation(notation, out move))
            {
                move._notation = notation;
                return move;
            }
            else
            {
                throw new Exception("Bad move notation");
            }
        }

        public static Move GetMove(string notation, Board board)
        {
            Move move = GetMove(notation);
            move.SimplifyMove(board);
            return move;
        }

        public void SimplifyMove(Board board)
        {
            if (_from == Hex.InvalidHex) return;
            Hex neighborHex = Hex.DirectionHex(_from, _to) + _from;
            if (board.Cells[neighborHex].Piece.NumPieces == 0)
            {
                _from = Hex.InvalidHex;
                _to = neighborHex;
            }
        }

        public Move(Hex from, Hex to, List<Hex> removeBefore, List<Hex> removeAfter, bool isGipf)
        {
            _isGipf = isGipf;
            _from = from;
            _to = to;
            _removeBefore = removeBefore;
            _removeAfter = removeAfter;
            if (null == _removeBefore) _removeBefore = new List<Hex>();
            if (null == _removeAfter) _removeAfter = new List<Hex>();
        }

        public Move(Hex to, bool isGipf) : this(Hex.InvalidHex, to, null, null, isGipf) { }

        public Move(Hex from, Hex to, bool isGipf)  : this(from, to, null, null, isGipf) { }

        public override bool Equals(object obj)
        {
            if (!(obj is Move)) return false;
            return Equals((Move)obj);
        }

        public bool Equals(Move other)
        {
            if(_from == other._from 
                && _to == other._to 
                && _isGipf == other._isGipf)
            {
                if (!ScrambledEquals(_removeBefore, other._removeBefore)) return false;
                else if (!ScrambledEquals(_removeAfter, other._removeAfter)) return false;
                else return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hashCode = _from.GetHashCode() * 7 + _to.GetHashCode() * 23 + _isGipf.GetHashCode();
            foreach (Hex hex in _removeBefore) hashCode += hex.GetHashCode();
            foreach (Hex hex in _removeAfter) hashCode += hex.GetHashCode();
            return hashCode;
        }

        public static bool operator !=(Move move1, Move move2) => !move1.Equals(move2);

        public static bool operator ==(Move move1, Move move2) => move1.Equals(move2);

        // http://stackoverflow.com/questions/3669970/compare-two-listt-objects-for-equality-ignoring-order
        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
    }
}