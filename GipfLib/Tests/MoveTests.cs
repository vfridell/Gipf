using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using GipfLib.Models;
using System;

namespace GipfLib.Tests
{
    [TestClass]
    public class MoveTests
    {
        [TestMethod]
        public void SimpleMoveEquivalency()
        {
            Board board = Board.GetInitialBoard();
            Move move1 = Move.GetMove(@"Ge8", board);
            Move move2 = Move.GetMove(@"Ge9-e8", board);
            Move move3 = Move.GetMove(@"Gd8-e8", board);
            Move move4 = Move.GetMove(@"Gf8-e8", board);

            Assert.IsTrue(move1 == move2);
            Assert.IsTrue(move1 == move3);
            Assert.IsTrue(move1 == move4);
            Assert.IsTrue(move2 == move3);
            Assert.IsTrue(move3 == move4);
            Assert.IsTrue(move2 == move4);
        }

        [TestMethod]
        public void SimpleMoveNonEquivalency()
        {
            Board board = Board.GetInitialBoard();
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Ge8")));
            Assert.IsTrue(board.TryMakeMove(Move.GetMove(@"Gd2")));

            Move move1 = Move.GetMove(@"Ge9-e8", board);
            Move move2 = Move.GetMove(@"Gd8-e8", board);
            Move move3 = Move.GetMove(@"Gf8-e8", board);

            Assert.IsFalse(move1 == move2);
            Assert.IsFalse(move1 == move3);
            Assert.IsFalse(move2 == move3);
        }

        [TestMethod]
        public void RemoveBeforeMoveEquivalency()
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

            Move move1 = Move.GetMove(@"xe7,e8;e2", board);
            Move move2 = Move.GetMove(@"xe7,e8;d1-e2", board);
            Move move3 = Move.GetMove(@"xe8,e7;f1-e2", board);

            Assert.IsTrue(move1 == move2);
            Assert.IsTrue(move1 == move3);
            Assert.IsTrue(move2 == move3);
        }

        [TestMethod]
        public void RemoveAfterMoveEquivalency()
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

            Move move1 = Move.GetMove(@"h6-c4;xc6*,d6,f5,g4", board);
            Move move2 = Move.GetMove(@"h6-c4;xc6*,d6,g4,f5", board);
            Move move3 = Move.GetMove(@"h6-c4;xc6*,f5,g4,d6", board);
            Move move4 = Move.GetMove(@"h6-c4;xc6*,g4,f5,d6", board);

            Assert.IsTrue(move1 == move2);
            Assert.IsTrue(move1 == move3);
            Assert.IsTrue(move1 == move4);
            Assert.IsTrue(move2 == move3);
            Assert.IsTrue(move3 == move4);
            Assert.IsTrue(move2 == move4);

            Move move5 = Move.GetMove(@"h6-c4;xc6*,d6,f5,g4,e6", board);
            Assert.IsFalse(move5 == move1);
        }
    }
}