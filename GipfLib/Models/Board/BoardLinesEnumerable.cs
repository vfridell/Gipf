using System;
using System.Collections;
using System.Collections.Generic;

namespace GipfLib.Models
{
    public class BoardLinesEnumerable : IEnumerable<Cell>
    {
        private Board _board;

        public BoardLinesEnumerable(Board board)
        {
            _board = board;
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            bool firstCell = true;
            int currentIndex = 0;
            Position currentDirection = _lineStarts[currentIndex].Item2;
            Cell currentCell = _board.Cells[_lineStarts[currentIndex].Item1];

            do
            {
                if (firstCell)
                {
                    firstCell = false;
                    yield return currentCell;
                }
                else if (currentCell is Wall)
                {
                    currentIndex++;
                    currentDirection = _lineStarts[currentIndex].Item2;
                    currentCell = _board.Cells[_lineStarts[currentIndex].Item1];
                    yield return currentCell;
                }
                else
                {
                    if (!currentCell.TryGetNeighbor(currentDirection, out currentCell))
                    {
                        throw new Exception($"No neighbor in direction {currentDirection.ToString()}");
                    }
                    else
                    {
                        yield return currentCell;
                    }
                }
            } while (!(currentCell is Wall && currentIndex + 1 >= _lineStarts.Count));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static readonly List<Tuple<Hex, Position>> _lineStarts = new List<Tuple<Hex, Position>>()
        {
            new Tuple<Hex, Position>(NotationParser.GetHex("e2"), Position.topright),
            new Tuple<Hex, Position>(NotationParser.GetHex("d2"), Position.topright),
            new Tuple<Hex, Position>(NotationParser.GetHex("c2"), Position.topright),
            new Tuple<Hex, Position>(NotationParser.GetHex("b2"), Position.topright),
            new Tuple<Hex, Position>(NotationParser.GetHex("b3"), Position.topright),
            new Tuple<Hex, Position>(NotationParser.GetHex("b4"), Position.topright),
            new Tuple<Hex, Position>(NotationParser.GetHex("b5"), Position.topright),
            new Tuple<Hex, Position>(NotationParser.GetHex("e8"), Position.bottomright),
            new Tuple<Hex, Position>(NotationParser.GetHex("d7"), Position.bottomright),
            new Tuple<Hex, Position>(NotationParser.GetHex("c6"), Position.bottomright),
            new Tuple<Hex, Position>(NotationParser.GetHex("b5"), Position.bottomright),
            new Tuple<Hex, Position>(NotationParser.GetHex("b4"), Position.bottomright),
            new Tuple<Hex, Position>(NotationParser.GetHex("b3"), Position.bottomright),
            new Tuple<Hex, Position>(NotationParser.GetHex("b2"), Position.bottomright),
            new Tuple<Hex, Position>(NotationParser.GetHex("b2"), Position.top),
            new Tuple<Hex, Position>(NotationParser.GetHex("c2"), Position.top),
            new Tuple<Hex, Position>(NotationParser.GetHex("d2"), Position.top),
            new Tuple<Hex, Position>(NotationParser.GetHex("e2"), Position.top),
            new Tuple<Hex, Position>(NotationParser.GetHex("f2"), Position.top),
            new Tuple<Hex, Position>(NotationParser.GetHex("g2"), Position.top),
            new Tuple<Hex, Position>(NotationParser.GetHex("h2"), Position.top),
        };
    }
}
