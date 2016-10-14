using System;
using System.Collections.Generic;

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

        public Move(Hex from, Hex to, List<Hex> removeBefore, List<Hex> removeAfter, bool isGipf)
        {
            _isGipf = isGipf;
            _from = from;
            _to = to;
            _removeBefore = removeBefore;
            _removeAfter = removeAfter;
        }

        public Move(Hex to, bool isGipf) : this(Hex.InvalidHex, to, null, null, isGipf) { }

        public Move(Hex from, Hex to, bool isGipf)  : this(from, to, null, null, isGipf) { }
    }
}