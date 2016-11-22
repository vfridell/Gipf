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

        List<RemoveMovePart> _removeBeforeParts;
        List<RemoveMovePart> _removeAfterParts;

        public bool isGipf => _isGipf;
        public bool isPlacement => _from == Hex.InvalidHex;
        public Hex from => _from;
        public Hex to => _to;
        public IReadOnlyList<Hex> removeBefore
        {
            get
            {
                List<Hex> removeList = new List<Hex>();
                _removeBeforeParts.ForEach(r => removeList.AddRange(r.HexesToRemove));
                return removeList.AsReadOnly();
            }
        }
        public IReadOnlyList<Hex> removeAfter 
        {
            get
            {
                List<Hex> removeList = new List<Hex>();
                _removeAfterParts.ForEach(r => removeList.AddRange(r.HexesToRemove));
                return removeList.AsReadOnly();
            }
        }

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

        public Move(Hex from, Hex to, List<RemoveMovePart> removeBefore, List<RemoveMovePart> removeAfter, bool isGipf)
        {
            _isGipf = isGipf;
            _from = from;
            _to = to;
            _removeBeforeParts = removeBefore;
            _removeAfterParts = removeAfter;
            if (null == _removeBeforeParts) _removeBeforeParts = new List<RemoveMovePart>();
            if (null == _removeAfterParts) _removeAfterParts = new List<RemoveMovePart>();
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
            bool eq = _from == other._from
                    && _to == other._to
                    && _isGipf == other._isGipf;
            if (!eq) return false;

            if (false == Helpers.ScrambledEquals(_removeBeforeParts, other._removeBeforeParts)) return false;
            if (false == Helpers.ScrambledEquals(_removeAfterParts, other._removeAfterParts)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return _from.GetHashCode() * 7 + _to.GetHashCode() * 23 + _isGipf.GetHashCode() 
                + _removeAfterParts.GetHashCode() + _removeBeforeParts.GetHashCode();
        }

        public static bool operator !=(Move move1, Move move2) => !move1.Equals(move2);

        public static bool operator ==(Move move1, Move move2) => move1.Equals(move2);
    }
}