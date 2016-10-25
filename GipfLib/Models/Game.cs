using System;
using System.Collections.Generic;

namespace GipfLib.Models
{
    public enum GameResult { Incomplete, WhiteWin, BlackWin, Draw }

    public class Game
    {
        private List<Move> _movesMade = new List<Move>();
        public IReadOnlyList<Move> movesMade => _movesMade.AsReadOnly();

        private List<Board> _boards = new List<Board>();
        public IReadOnlyList<Board> boards => _boards.AsReadOnly();

        private Board _currentBoard;

        public readonly string whitePlayerName;
        public readonly string blackPlayerName;

        public bool whiteToPlay { get { return _currentBoard.whiteToPlay; } }
        public GameResult gameResult { get { return _currentBoard.gameResult; } }
        //public string lastError { get { return _currentBoard.lastError; } }
        public int turnNumber { get { return _currentBoard.turnNumber; } }
        //public int currentBoardIndex { get { return _currentBoard.turnNumber - 2; } }

        private Game(string whitePlayerName, string blackPlayerName)
        {
            this.whitePlayerName = whitePlayerName;
            this.blackPlayerName = blackPlayerName;
        }

        public bool TryMakeMove(Move move)
        {
            //move.FixNotation(_currentBoard);
            //if (_currentBoard.TryMakeMove(move))
            //{
            //    _boards.Add(_currentBoard.Clone());
            //    _movesMade.Add(move);
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            throw new NotImplementedException();
        }

        public bool ThreeFoldRepetition()
        {
            //int repetitions = 0;
            //// first board in _boards is same as _currentBoard (see TryMakeMove() above)
            //// second board is last turn, which cannot be the same
            //foreach (Board thisOldBoard in _boards.OrderByDescending(b => b.turnNumber).Skip(2))
            //{
            //    if (Board.BoardPositionsEqual(_currentBoard, thisOldBoard)) repetitions++;
            //    if (repetitions == 3) return true;
            //}
            //return false;
            throw new NotImplementedException();
        }

        public Board GetCurrentBoard()
        {
            return _currentBoard.Clone();
        }

        public static Game GetNewGame(string whitePlayerName, string blackPlayerName)
        {
            Game game = new Game(whitePlayerName, blackPlayerName);
            game._currentBoard = Board.GetInitialBoard();
            return game;
        }

        public string GetMoveTranscript()
        {
            //StringBuilder sb = new StringBuilder();
            //_movesMade.ForEach(m => sb.Append(m.notation).Append("\n"));
            //return sb.ToString();
            throw new NotImplementedException();
        }

        public static string WriteGameTranscript(Game game)
        {
            //string filename = string.Format("transcript_{0}", DateTime.Now.ToString("yyyy.MM.dd.HHmmss"));
            //using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filename + ".txt"))
            //{
            //    writer.Write(game.GetMoveTranscript());
            //}

            //BinaryFormatter formatter = new BinaryFormatter();
            //using (Stream stream = new FileStream(filename + ".bin", FileMode.Create, FileAccess.Write, FileShare.None))
            //{
            //    formatter.Serialize(stream, game);
            //}
            //return filename;
            throw new NotImplementedException();
        }
    }
}

