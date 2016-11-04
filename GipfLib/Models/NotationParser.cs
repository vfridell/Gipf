using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GipfLib.Models
{
    public static class NotationParser
    {
        private static Regex _pushRegex = new Regex(@"^(G?[a-i][1-9])(-([a-i][1-9]))?");
        private static Regex _captureFullRegex = new Regex(@"^x(((G?[a-i][1-9])\*?)(,(G?[a-i][1-9])\*?)*)");

        public static Hex GetHex(string gipfCoordinate)
        {
            if (string.IsNullOrEmpty(gipfCoordinate)) return Hex.InvalidHex;

            int startIndex = 0;
            if (gipfCoordinate[0] == 'G') startIndex = 1;

            ColumnLabelEnum columnLabel;
            if (!Enum.TryParse<ColumnLabelEnum>(gipfCoordinate.Substring(startIndex, 1), out columnLabel))
            {
                throw new Exception($"Bad column coordinate notation: {gipfCoordinate}");
            }

            int column = (int)columnLabel;

            int rowLabel;
            if (!int.TryParse(gipfCoordinate.Substring(startIndex + 1, 1), out rowLabel))
            {
                throw new Exception($"Bad row coordinate notation: {gipfCoordinate}");
            }

            int row = column <= 0 ? (5 - rowLabel) : (5 - (rowLabel + column));

            return new Hex(column, row);
        }

        internal static bool TryParseNotation(string notation, out Move move)
        {
            try
            {
                List<RemoveMovePart> removeBefores = new List<RemoveMovePart>();
                List<RemoveMovePart> removeAfters = new List<RemoveMovePart>();
                string[] notationParts = notation.Split(';');
                bool pushSeen = false;
                Hex fromCoord = Hex.InvalidHex;
                Hex toCoord = Hex.InvalidHex;
                bool isGipf = false;
                foreach(string notationPart in notationParts)
                {
                    RemoveMovePart movePart;
                    if(TryMakeRemoveList(notationPart, out movePart))
                    {
                        if (pushSeen) removeAfters.Add(movePart);
                        else removeBefores.Add(movePart);
                    }
                    else
                    {
                        if (pushSeen) throw new Exception("Multiple pushes in notation");
                        ParsePush(notationPart, out fromCoord, out toCoord, out isGipf);
                        pushSeen = true;
                    }
                }
                if (!pushSeen) throw new Exception("No push in notation");
                move = new Move(fromCoord, toCoord, removeBefores, removeAfters, isGipf);
                return true;
            }
            catch(Exception)
            {
                move = null;
                return false;
            }
        }

        internal static bool TryMakeRemoveList(string notation, out RemoveMovePart movePart)
        {
            MatchCollection captureMatches;
            captureMatches = _captureFullRegex.Matches(notation);
            if (captureMatches.Count == 0)
            {
                movePart = null;
                return false;
            }

            List<Hex> captureList = captureMatches[0].Groups[1].Value.Replace("*", "").Split(',').Select(s => GetHex(s)).ToList();
            movePart = new RemoveMovePart(captureList);
            return true;
        }

        private static void ParsePush(string notation, out Hex fromCoord, out Hex toCoord, out bool isGipf)
        {
            if (!_pushRegex.IsMatch(notation)) throw new Exception("Bad Notation: invalid push notation");
            MatchCollection matches = _pushRegex.Matches(notation);
            isGipf = matches[0].Groups[1].Value[0] == 'G';
            fromCoord = GetHex(matches[0].Groups[1].Value);
            toCoord = GetHex(matches[0].Groups[3].Value);

            if (toCoord == Hex.InvalidHex)
            {
                toCoord = fromCoord;
                fromCoord = Hex.InvalidHex;
            }
        }

        private enum ColumnLabelEnum { a = -4, b = -3, c = -2, d = -1, e = 0, f = 1, g = 2, h = 3, i = 4 };
        public static string GetGipfCoordinate(Hex hex)
        {
            string columnLabel = ((ColumnLabelEnum)hex.column).ToString();
            string rowLabel = hex.column <= 0 ? (5 - hex.row).ToString() : (5 - (hex.row + hex.column)).ToString();
            return string.Concat(columnLabel, rowLabel);
        }

    }
}
