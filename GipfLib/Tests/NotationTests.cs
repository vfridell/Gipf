using Microsoft.VisualStudio.TestTools.UnitTesting;
using GipfLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GipfLib.Tests
{
    [TestClass]
    public class NotationTests
    {
        [TestMethod]
        public void TestHexToGipfCoordinates()
        {
            Assert.AreEqual("e5", NotationParser.GetGipfCoordinate(new Hex(0, 0)));
            Assert.AreEqual("a1", NotationParser.GetGipfCoordinate(new Hex(-4, 4)));
            Assert.AreEqual("i1", NotationParser.GetGipfCoordinate(new Hex(4, 0)));
            Assert.AreEqual("e9", NotationParser.GetGipfCoordinate(new Hex(0, -4)));
            Assert.AreEqual("c5", NotationParser.GetGipfCoordinate(new Hex(-2, 0)));
        }

        [TestMethod]
        public void TestGipfCoordinatesToHex()
        {
            Assert.AreEqual(new Hex(0, 0), NotationParser.GetHex("e5"));
            Assert.AreEqual(new Hex(-4, 4), NotationParser.GetHex("a1"));
            Assert.AreEqual(new Hex(4, 0), NotationParser.GetHex("i1"));
            Assert.AreEqual(new Hex(0, -4), NotationParser.GetHex("e9"));
            Assert.AreEqual(new Hex(-2, 0), NotationParser.GetHex("c5"));
        }

        [TestMethod]
        public void ParseMoveNotationPushToOnly()
        {
            Move move1;
            Assert.IsTrue(NotationParser.TryParseNotation("Ge8", out move1));
            Assert.IsTrue(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(0, -3));
            Assert.AreEqual(move1.from, Hex.InvalidHex);

            Assert.IsTrue(NotationParser.TryParseNotation("e2", out move1));
            Assert.IsFalse(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(0, 3));
            Assert.AreEqual(move1.from, Hex.InvalidHex);

            Assert.IsTrue(NotationParser.TryParseNotation("c6", out move1));
            Assert.IsFalse(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(-2, -1));
            Assert.AreEqual(move1.from, Hex.InvalidHex);

            Assert.IsTrue(NotationParser.TryParseNotation("Gb5", out move1));
            Assert.IsTrue(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(-3, 0));
            Assert.AreEqual(move1.from, Hex.InvalidHex);
        }

        [TestMethod]
        public void ParseMoveNotationPushFromTo()
        {
            Move move1;
            Assert.IsTrue(NotationParser.TryParseNotation("Gd8-f7", out move1));
            Assert.IsTrue(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(1, -3));
            Assert.AreEqual(move1.from, new Hex(-1, -3));

            Assert.IsTrue(NotationParser.TryParseNotation("b6-f5", out move1));
            Assert.IsFalse(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(1, -1));
            Assert.AreEqual(move1.from, new Hex(-3, -1));

            Assert.IsTrue(NotationParser.TryParseNotation("a3-e7", out move1));
            Assert.IsFalse(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(0, -2));
            Assert.AreEqual(move1.from, new Hex(-4, 2));

            Assert.IsTrue(NotationParser.TryParseNotation("Ge9-e6", out move1));
            Assert.IsTrue(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(0, -1));
            Assert.AreEqual(move1.from, new Hex(0, -4));
        }

        [TestMethod]
        public void ParseMoveNotationPushCapture()
        {
            Move move1;
            Assert.IsTrue(NotationParser.TryParseNotation("h6-c4;xc6*,d6,f5,g4", out move1));
            Assert.IsFalse(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(-2, 1));
            Assert.AreEqual(move1.from, new Hex(3, -4));
            Assert.AreEqual(4, move1.removeAfter.Count);
            Assert.AreEqual(0, move1.removeBefore.Count);

            Assert.IsTrue(NotationParser.TryParseNotation("a5-g3;xb3*,c4*,d5,g6", out move1));
            Assert.IsFalse(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(2, 0));
            Assert.AreEqual(move1.from, new Hex(-4, 0));
            Assert.AreEqual(4, move1.removeAfter.Count);
            Assert.AreEqual(0, move1.removeBefore.Count);
        }

        [TestMethod]
        public void ParseMoveNotationCapturePush()
        {
            Move move1;
            Assert.IsTrue(NotationParser.TryParseNotation("xe7,e8;c7-e7", out move1));
            Assert.IsFalse(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(0, -2));
            Assert.AreEqual(move1.from, new Hex(-2, -2));
            Assert.AreEqual(2, move1.removeBefore.Count);
            Assert.AreEqual(0, move1.removeAfter.Count);

            Assert.IsTrue(NotationParser.TryParseNotation("xd7*,e7,f6,h4;a5-f4", out move1));
            Assert.IsFalse(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(1, 0));
            Assert.AreEqual(move1.from, new Hex(-4, 0));
            Assert.AreEqual(4, move1.removeBefore.Count);
            Assert.AreEqual(0, move1.removeAfter.Count);
        }

        [TestMethod]
        public void ParseMoveNotationCapturePushCapture()
        {
            Move move1;
            Assert.IsTrue(NotationParser.TryParseNotation("xe3,e2,e6;h1-b4;xe4,b4,f3,g2,Gd4,Gc4", out move1));
            Assert.IsFalse(move1.isGipf);
            Assert.AreEqual(move1.to, new Hex(-3, 1));
            Assert.AreEqual(move1.from, new Hex(3, 1));
            Assert.AreEqual(3, move1.removeBefore.Count);
            Assert.AreEqual(6, move1.removeAfter.Count);
        }
    }
}
