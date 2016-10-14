using System;
using System.Collections;
using System.Collections.Generic;

namespace GipfLib.Models
{
    public class BoardWallsEnumerable : IEnumerable<Cell>
    {
        private Board _board;
        private Position _currentDirection;
        private Cell _currentCell;

        public BoardWallsEnumerable(Board board)
        {
            _board = board;
            _currentDirection = _corners[NotationParser.GetHex("a1")];
            _currentCell = board.Cells[NotationParser.GetHex("a1")];
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            bool seenAllWalls = false;
            bool firstCell = true;

            while (!seenAllWalls)
            {
                if (firstCell)
                {
                    firstCell = false;
                    yield return _currentCell;
                }
                else 
                {
                    if (!_currentCell.TryGetNeighbor(_currentDirection, out _currentCell))
                    {
                        throw new Exception($"No neighbor in direction {_currentDirection.ToString()}");
                    }

                    Position newDirection;
                    if(_corners.TryGetValue(_currentCell.hex, out newDirection))
                    {
                        _currentDirection = newDirection;
                    }

                    if (_currentCell.hex == NotationParser.GetHex("a1"))
                    {
                        seenAllWalls = true;
                    }
                    else
                    {
                        yield return _currentCell;
                    }
                }
            } 
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static readonly Dictionary<Hex, Position> _corners = new Dictionary<Hex, Position>()
        {
            [NotationParser.GetHex("a1")] = Position.bottomright,
            [NotationParser.GetHex("e1")] = Position.topright,
            [NotationParser.GetHex("i1")] = Position.top,
            [NotationParser.GetHex("i5")] = Position.topleft,
            [NotationParser.GetHex("e9")] = Position.bottomleft,
            [NotationParser.GetHex("a5")] = Position.bottom,
        };
    }
}
