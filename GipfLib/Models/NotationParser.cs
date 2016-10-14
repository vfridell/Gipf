using System;
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
                string[] notationParts = notation.Split(';');
                switch (notationParts.Length)
                {
                    case 1:
                        move = ParsePushOnlyMove(notation);
                        break;
                    case 2:
                        move = ParsePushCaptureMove(notationParts);
                        break;
                    case 3:
                        move = ParseCapturePushCaptureMove(notationParts);
                        break;
                    default:
                        throw new Exception("Bad Notation: Too many semicolons");
                }
                return true;
            }
            catch(Exception)
            {
                move = null;
                return false;
            }
        }


        private static Move ParsePushOnlyMove(string notation)
        {
            if (!_pushRegex.IsMatch(notation)) throw new Exception("Bad Notation: invalid push notation");
            MatchCollection matches = _pushRegex.Matches(notation);
            string fromCoord = matches[0].Groups[1].Value;
            string toCoord = matches[0].Groups[3].Value;

            if(string.IsNullOrEmpty(toCoord))
            {
                return new Move(GetHex(fromCoord), fromCoord[0] == 'G');
            }
            else
            {
                return new Move(GetHex(fromCoord), GetHex(toCoord), fromCoord[0] == 'G');
            }
        }

        private static Move ParsePushCaptureMove(string [] notationParts)
        {
            bool captureFirst;
            MatchCollection pushMatches;
            MatchCollection captureMatches;
            if (_pushRegex.IsMatch(notationParts[0]))
            {
                pushMatches = _pushRegex.Matches(notationParts[0]);
                captureMatches = _captureFullRegex.Matches(notationParts[1]);
                captureFirst = false;
            }
            else
            {
                captureMatches = _captureFullRegex.Matches(notationParts[0]);
                pushMatches = _pushRegex.Matches(notationParts[1]);
                captureFirst = true;
            }
            if (pushMatches.Count == 0) throw new Exception($"Bad push notation: {notationParts[0]}{notationParts[1]}");
            if (captureMatches.Count == 0) throw new Exception($"Bad capture notation: {notationParts[0]}{notationParts[1]}");

            string fromCoord = pushMatches[0].Groups[1].Value;
            string toCoord = pushMatches[0].Groups[3].Value;
            string[] captureList = captureMatches[0].Groups[1].Value.Replace("*", "").Split(',');

            if (string.IsNullOrEmpty(toCoord))
            {
                if(captureFirst)
                {
                    return new Move(Hex.InvalidHex, GetHex(fromCoord), captureList.Select(s => GetHex(s)).ToList(), null, fromCoord[0] == 'G');
                }
                else
                {
                    return new Move(Hex.InvalidHex, GetHex(fromCoord), null, captureList.Select(s => GetHex(s)).ToList(), fromCoord[0] == 'G');
                }
            }
            else
            {
                if (captureFirst)
                {
                    return new Move(GetHex(fromCoord), GetHex(toCoord), captureList.Select(s => GetHex(s)).ToList(), null, fromCoord[0] == 'G');
                }
                else
                {
                    return new Move(GetHex(fromCoord), GetHex(toCoord), null, captureList.Select(s => GetHex(s)).ToList(), fromCoord[0] == 'G');
                }
            }
        }

        private static Move ParseCapturePushCaptureMove(string [] notationParts)
        {
            MatchCollection pushMatches;
            MatchCollection captureMatchesBefore;
            MatchCollection captureMatchesAfter;
            captureMatchesBefore = _captureFullRegex.Matches(notationParts[0]);
            pushMatches = _pushRegex.Matches(notationParts[1]);
            captureMatchesAfter = _captureFullRegex.Matches(notationParts[2]);
            if (pushMatches.Count == 0) throw new Exception($"Bad push notation: {notationParts[0]}{notationParts[1]}{notationParts[2]}");
            if (captureMatchesAfter.Count == 0) throw new Exception($"Bad capture after notation: {notationParts[0]}{notationParts[1]}{notationParts[2]}");
            if (captureMatchesBefore.Count == 0) throw new Exception($"Bad capture before notation: {notationParts[0]}{notationParts[1]}{notationParts[2]}");

            string fromCoord = pushMatches[0].Groups[1].Value;
            string toCoord = pushMatches[0].Groups[3].Value;
            string[] captureListBefore = captureMatchesBefore[0].Groups[1].Value.Replace("*", "").Split(',');
            string[] captureListAfter = captureMatchesAfter[0].Groups[1].Value.Replace("*", "").Split(',');

            if (string.IsNullOrEmpty(toCoord))
            {
                return new Move(Hex.InvalidHex, GetHex(fromCoord), captureListBefore.Select(s => GetHex(s)).ToList(), captureListAfter.Select(s => GetHex(s)).ToList(), fromCoord[0] == 'G');
            }
            else
            {
                return new Move(GetHex(fromCoord), GetHex(toCoord), captureListBefore.Select(s => GetHex(s)).ToList(), captureListAfter.Select(s => GetHex(s)).ToList(), fromCoord[0] == 'G');
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
