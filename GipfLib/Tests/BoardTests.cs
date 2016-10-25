using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using GipfLib.Models;
using System;

namespace GipfLib.Tests
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void OutsideIsOnlyWalls()
        {
            Board board = Board.GetInitialBoard();
            IEnumerable<KeyValuePair<Hex, Cell>> shouldOnlyBeWalls = board.Cells.Where(kvp => kvp.Value.NeighborhoodCells.Count != 6);
            Assert.AreEqual(24, shouldOnlyBeWalls.Select(kvp => kvp.Value).Where(cell => cell is Wall).Count());
        }

        [TestMethod]
        public void FindRunsVisitor()
        {
            Board board = Board.GetInitialBoard();
            Cell cell = board.Cells[NotationParser.GetHex("a1")];
            cell.Push(Position.topright, Pieces.White);
            cell.Push(Position.topright, Pieces.Black);
            cell.Push(Position.topright, Pieces.Black);
            cell.Push(Position.topright, Pieces.Black);
            cell.Push(Position.topright, Pieces.Black);

            cell = board.Cells[NotationParser.GetHex("i3")];
            cell.Push(Position.topleft, Pieces.BlackGipf);
            cell.Push(Position.topleft, Pieces.White);
            cell.Push(Position.topleft, Pieces.White);
            cell.Push(Position.topleft, Pieces.White);
            cell.Push(Position.topleft, Pieces.White);

            var analyzer = new BoardAnalyzer(board.GetLinesEnumerable());
            var visitor = new FindRunsVisitor();
            analyzer.AddVisitor(visitor);
            analyzer.AnalyzeBoard();

            Assert.AreEqual(2, visitor.RunsOf4.Count);
            Assert.AreEqual(PieceColor.Black, visitor.RunsOf4[0][0].Piece.Color);
            Assert.AreEqual(PieceColor.White, visitor.RunsOf4[1][0].Piece.Color);
        }

        [TestMethod]
        public void EnumerateWalls()
        {
            Board board = Board.GetInitialBoard();
            IEnumerable<Cell> walls = board.GetWallsEnumerable();
            Assert.AreEqual(24, walls.Count());
            Assert.AreEqual(24, walls.Count(w => w is Wall));
        }

        [TestMethod]
        public void EnumerateLines()
        {
            Board board = Board.GetInitialBoard();
            IEnumerable<Cell> linesEnumerable = board.GetLinesEnumerable();

            // we enumerate a wall at the end of every line of cells.  So:
            // (5 + 6 + 7 + 8 + 7 + 6 + 5) * 3 = 132
            Assert.AreEqual(132, linesEnumerable.Count());

            // just the non-walls:
            // (4 + 5 + 6 + 7 + 6 + 5 + 4) * 3 = 111
            Assert.AreEqual(111, linesEnumerable.Count(c => !(c is Wall)));
        }

        [TestMethod]
        public void ApplyMovesToBoard()
        {
            Board board = Board.GetInitialBoard();
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Ge8")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gd8-f7")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Ge9-e7")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gf8-d7")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Ge9-e6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gd8-g6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"g7-d6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f8-c6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f8-f6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"c7-g5")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"g7-c5")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"e9-e5")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"b6-f5")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"xe7,e8;c7-e7")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"g7-b4")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"c7-h4")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"h6-d5")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"b6-g4")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"h6-c4;xc6*,d6,f5,g4")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"b3")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"b5")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"g7-d6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"xd7*,e7,f6,h4;a5-f4")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a2-f6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a5-g3;xb3*,c4*,d5,g6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"e2")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f2")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"d1-g2")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"h1-f3")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"d1-h2")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f1-f5")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"i1-d5")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"g6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"g1-e3;xf2,f3,f5,Gf6*,f7")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"h6-f6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a3-e7")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"h6-c4;xGc4*,e6,f6,g6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"e8")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f1-d2")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"c6")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"c7-c4")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"g6")));
            Assert.AreEqual(0, board.reserveBlackPieces);
            Assert.AreEqual(5, board.reserveWhitePieces);
            Assert.AreEqual(GameResult.WhiteWin, board.gameResult);
            Assert.AreEqual(2, board.blackGipfPiecesInPlay);
            Assert.AreEqual(2, board.whiteGipfPiecesInPlay);
        }

        [TestMethod]
        public void RemoveBeforeList()
        {
            Board board = Board.GetInitialBoard();
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gb2")));     //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gf2")));     //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gb5")));     //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gf1-f3")));  //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a1-c3")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f1-f4")));   //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a1-d4")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a5-c5")));   //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f1-f5")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f7")));      //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a5-d5")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a5-e5")));   //b

            Assert.AreEqual(10, board.reserveBlackPieces);
            Assert.AreEqual(10, board.reserveWhitePieces);
            Assert.AreEqual(GameResult.Incomplete, board.gameResult);
            Assert.AreEqual(2, board.blackGipfPiecesInPlay);
            Assert.AreEqual(2, board.whiteGipfPiecesInPlay);

            Assert.AreEqual(4, board.AllPossibleRemoveLists.Count);
        }

        [TestMethod]
        public void ExceptionIfNoPostRemove()
        {
            Board board = Board.GetInitialBoard();
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gb2")));     //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gf2")));     //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gb5")));     //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gf1-f3")));  //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a1-c3")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f1-f4")));   //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a1-d4")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a5-c5")));   //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f1-f5")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f7")));      //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a5-d5")));   //w
            Assert.IsFalse(board.TryMakeMove(Move.GetMove(@"f8-f6")));   //b
            Assert.IsTrue(board.LastError.Contains("Post-push removal did not clear all extended runs of four of current player's color"));
        }

        [TestMethod]
        public void GetAllStartingTournamentMoves()
        {
            Board board = Board.GetInitialBoard(GameType.Tournament);
            Assert.AreNotEqual(board.GetMoves()[0], board.GetMoves()[1]);
            Assert.AreEqual(18, board.GetMoves().Count);
            Assert.AreEqual(0, board.GetMoves().Where(m => m.isGipf == false).Count());
        }

        [TestMethod]
        public void GetAllStartingStandardMoves()
        {
            Board board = Board.GetInitialBoard(GameType.Standard);
            Assert.AreNotEqual(board.GetMoves()[0], board.GetMoves()[1]);
            Assert.AreEqual(30, board.GetMoves().Count);
            Assert.AreEqual(30, board.GetMoves().Where(m => m.isGipf == false).Count());
        }

        [TestMethod]
        public void GetMovesWithRemoval()
        {
            Board board = Board.GetInitialBoard(GameType.Tournament);
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gb2")));     //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gf2")));     //b
            // 21 possible pushes * 2 (Gipf or non-Gipf both possible)
            Assert.AreEqual(42, board.GetMoves().Count);
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gb5")));     //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gf1-f3")));  //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a1-c3")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f1-f4")));   //b
            Assert.AreEqual(0, board.GetMoves().Where(m => m.isGipf == true).Count());
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a1-d4")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a5-c5")));   //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f1-f5")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"f7")));      //b
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a5-d5")));   //w
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"a5-e5")));   //b
            IReadOnlyList<Move> moves = board.GetMoves().Where(m => m.removeBefore.Count > 0).ToList();
            // 4 possible remove befores * 22 possible pushes
            Assert.AreEqual(22 * 4, moves.Count);
        }
    }
}