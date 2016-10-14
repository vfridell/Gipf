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
        public void CheckAxisAligned()
        {
            Hex to = new Hex(2, 0, -2);
            Hex from = new Hex(-1, -3, 4);
            Assert.IsFalse(Hex.AxisAligned(from, to));

            Hex to2 = new Hex(3, -3, 0);
            Hex from2 = new Hex(-1, -3, 4);
            Assert.IsTrue(Hex.AxisAligned(from2, to2));
        }
    }
}
