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
    public class HexTests
    {
        [TestMethod]
        public void GetDirectionHex()
        {
            Hex to = new Hex(3, -3, 0);
            Hex from = new Hex(-1, -3, 4);

            Hex subtract = (to - from);
            int dist = Hex.Distance(from, to);
            Hex divide = subtract / dist;

            Hex result = Hex.DirectionHex(from, to);
            Hex expected = new Hex(1, 0, -1);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetPositonFromDirectionHex()
        {
            Hex to = new Hex(3, -3, 0);
            Hex from = new Hex(-1, -3, 4);

            Position result = Neighborhood.GetDirection(from, to);

            Assert.AreEqual(Position.topright, result);
        }

        [TestMethod]
        public void AxisAligned()
        {
            Hex to = new Hex(2, 0, -2);
            Hex from = new Hex(-1, -3, 4);
            Assert.IsFalse(Hex.AxisAligned(from, to));

            Hex to2 = new Hex(3, -3, 0);
            Hex from2 = new Hex(-1, -3, 4);
            Assert.IsTrue(Hex.AxisAligned(from2, to2));
        }

        [TestMethod]
        public void AxisAlignedList()
        {
            Hex[] hexes1 =
            {
                new Hex(0, 0, 0),
                new Hex(-1, 0, 1),
                new Hex(-4, 0, 4),
                new Hex(5, 0, -5),
            };
            Assert.IsTrue(Hex.AxisAligned(hexes1, Position.topright));

            Hex[] hexes2 =
            {
                new Hex(0, 0, 0),
                new Hex(-1, 0, 1),
                new Hex(0, 1, -1),
                new Hex(-4, 0, 4),
                new Hex(5, 0, -5),
            };
            Assert.IsFalse(Hex.AxisAligned(hexes2, Position.topright));

            Hex[] hexes3 =
            {
                new Hex(-3, 2, 1),
                new Hex(-3, 5, -2),
                new Hex(-3, 0, 3),
                new Hex(-2, -3, 5),
            };
            Assert.IsFalse(Hex.AxisAligned(hexes3, Position.bottomright));
        }

        [TestMethod]
        public void AxisAlignedSort()
        {
            Hex[] hexesPreSorted =
            {
                new Hex(-5, 2, 3),
                new Hex(-3, 2, 1),
                new Hex(-2, 2, 0),
                new Hex(0, 2, -2),
                new Hex(3, 2, -5),
            };
            Hex[] hexesUnsorted =
            {
                new Hex(0, 2, -2),
                new Hex(3, 2, -5),
                new Hex(-3, 2, 1),
                new Hex(-5, 2, 3),
                new Hex(-2, 2, 0),
            };

            Assert.IsFalse(hexesUnsorted.SequenceEqual(hexesPreSorted));
            Assert.IsTrue(Hex.SortOnAxis(hexesUnsorted, Position.topright).SequenceEqual(hexesPreSorted));
        }

        [TestMethod]
        public void IsContiguous()
        {
            Hex hex1 = new Hex(-2,2,0);
            Hex hex2 = new Hex(-3,2,1);
            Assert.IsTrue(Hex.IsContiguous(hex1, hex2, Position.bottomleft));
            Assert.IsFalse(Hex.IsContiguous(hex1, hex2, Position.top));

            Hex hex3 = new Hex(1,1,-2);
            Hex hex4 = new Hex(0,0,0);
            Assert.IsFalse(Hex.IsContiguous(hex3, hex4, Position.top));

            Hex hex5 = new Hex(-1, -3, 4);
            Hex hex6 = new Hex(0, -4, 4);
            Assert.IsFalse(Hex.IsContiguous(hex5, hex6, Position.top));
            Assert.IsTrue(Hex.IsContiguous(hex5, hex6, Position.topleft));

            Hex hex7 = new Hex(-5, 2, 3);
            Hex hex8 = new Hex(-4, 1, 3);
            Assert.IsTrue(Hex.IsContiguous(hex7, hex8, Position.topleft));
        }

        [TestMethod]
        public void IsContiguousRemoveMovePart()
        {
            Hex[] hexes =
            {
                new Hex(-5, 2, 3),
                new Hex(-3, 2, 1),
                new Hex(-2, 2, 0),
                new Hex(0, 2, -2),
                new Hex(3, 2, -5),
            };
            Assert.IsFalse(Hex.IsContiguous(hexes, Position.topright));

            Hex[] hexes2 =
            {
                new Hex(-2, -1, 3),
                new Hex(-5, 2, 3),
                new Hex(-3, 0, 3),
                new Hex(-1, -2, 3),
                new Hex(-4, 1, 3),
            };
            Assert.IsTrue(Hex.IsContiguous(hexes2, Position.bottomright));

            RemoveMovePart rmp = new RemoveMovePart(hexes2);
            Assert.IsFalse(rmp.ContiguousAfterRemoval(new Hex(-3, 0, 3)));
            Assert.IsTrue(rmp.ContiguousAfterRemoval(new Hex(-5, 2, 3)));
        }
    }
}
