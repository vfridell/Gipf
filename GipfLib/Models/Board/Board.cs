using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GipfLib.Models
{
    public class Board
    {
        static Hex start = new Hex(0, 0);
        static int boardRadius = 4;

        // serialize this stuff
        Dictionary<Hex, Cell> _cellMap = new Dictionary<Hex, Cell>();

        private List<int> _reservePieces = new List<int>(){ 18, 18 };
        private List<int> _capturedPieces = new List<int>(){ 0, 0 };
        public int reserveWhitePieces => _reservePieces[(int)PieceColor.White];
        public int reserveBlackPieces => _reservePieces[(int)PieceColor.Black];

        private bool _whiteToPlay = true;
        public bool whiteToPlay => _whiteToPlay;
        private PieceColor colorToPlay => _whiteToPlay ? PieceColor.White : PieceColor.Black;

        private GameResult _gameResult = GameResult.Incomplete;
        public GameResult gameResult => _gameResult;

        private int _turnNumber = 1;
        public int turnNumber => _turnNumber;

        // end serialize

        private bool _runsDirty = true;

        private GipfPieceCountVisitor _gipfCountVisitor = new GipfPieceCountVisitor();
        public int blackGipfPiecesInPlay 
        { get { if (_runsDirty) FindRuns(); return _gipfCountVisitor.BlackGipfCount; } }
        public int whiteGipfPiecesInPlay 
        { get { if (_runsDirty) FindRuns(); return _gipfCountVisitor.WhiteGipfCount; } }

        private FindRunsVisitor _runsVisitor = new FindRunsVisitor();
        public IReadOnlyList<IReadOnlyList<Cell>> Runs
        { get { if (_runsDirty) FindRuns(); return _runsVisitor.Runs; } }

        public IReadOnlyList<IReadOnlyList<Cell>> RunsOfFour 
        { get { if (_runsDirty) FindRuns(); return _runsVisitor.RunsOf4; } }

        public IReadOnlyList<IReadOnlyList<Cell>> ExtendedRunsOfFour 
        { get { if (_runsDirty) FindRuns(); return _runsVisitor.ExtendedRunsOf4; } }

        public string LastError => _lastError;
        private string _lastError;

        private void AddToReserve(PieceColor color, int amount) => _reservePieces[(int)color] += amount;
        private void RemoveFromReserve(PieceColor color, int amount) => _reservePieces[(int)color] -= amount;
        private void AddToCaptured(PieceColor color, int amount) => _capturedPieces[(int)color] += amount;

        private Board() { }

        public IReadOnlyDictionary<Hex, Cell> Cells => new ReadOnlyDictionary<Hex, Cell>(_cellMap);

        public IEnumerable<Cell> GetLinesEnumerable() => new BoardLinesEnumerable(this);

        public IReadOnlyCollection<Move> GetMoves()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Make a move.  Validates the move or fails.
        /// </summary>
        /// <param name="move"></param>
        public bool TryMakeMove(Move move)
        {
            try
            {
                if (_gameResult != GameResult.Incomplete) throw new Exception("This game is over");

                GipfPiece piece = Pieces.GetPiece(this, move);
                Position direction = Neighborhood.GetDirection(move.from, move.to);
                if (move.isPlacement)
                {
                    if(_cellMap[move.to].Piece.Color != PieceColor.None) throw new Exception($"Trying to place on top of an existing piece: {move.to.column}, {move.to.row}");
                }
                else
                {
                    if (!(_cellMap[move.from] is Wall)) throw new Exception($"Trying to push from a non-wall: {move.from.column}, {move.from.row} => {direction}");
                    if (!_cellMap[move.from].CanPush(direction)) throw new Exception($"Cannot push here: {move.from.column}, {move.from.row} => {direction}");
                }

                RemoveOrCapturePieces(move.removeBefore);

                if (ExtendedRunsOfFour.Count != 0) throw new Exception("Pre-push removal did not clear all extended runs of four");

                if (move.isPlacement)
                {
                    _cellMap[move.to].SetPiece(piece);
                }
                else
                {
                    _cellMap[move.from].Push(direction, piece);
                }
                _runsDirty = true;

                RemoveFromReserve(piece.Color, piece.NumPieces);

                RemoveOrCapturePieces(move.removeAfter);

                if (ExtendedRunsOfFour.Where(l => l[0].Piece.Color == colorToPlay).Count() != 0) throw new Exception($"Post-push removal did not clear all extended runs of four of current player's color ({colorToPlay})");

                IncrementTurn();
                return true;
            }
            catch(Exception ex)
            {
                _lastError = ex.Message;
                return false;
            }
        }

        private void RemoveOrCapturePieces(IReadOnlyList<Hex> hexes)
        {
            if (hexes == null) return;
            foreach (Hex hex in hexes)
            {
                GipfPiece pieceToRemove = _cellMap[hex].Piece;
                if (pieceToRemove.NumPieces == 0) throw new Exception($"Trying to remove from an empty cell: {hex.column}, {hex.row}");
                if(!RemovalValid(hex)) throw new Exception($"Piece at Hex not part of an extended run of four: {hex.column}, {hex.row}");
                if (pieceToRemove.Color == colorToPlay)
                {
                    AddToReserve(pieceToRemove.Color, pieceToRemove.NumPieces);
                }
                else
                {
                    AddToCaptured(pieceToRemove.Color, pieceToRemove.NumPieces);
                }
                _cellMap[hex].SetPiece(Pieces.NoPiece);
            }
            _runsDirty = true;
        }

        private void FindRuns()
        {
            var analyzer = new BoardAnalyzer(GetLinesEnumerable());
            analyzer.AddVisitor(_runsVisitor);
            analyzer.AddVisitor(_gipfCountVisitor);
            analyzer.AnalyzeBoard();
            _runsDirty = false;
        }

        private bool RemovalValid(Hex hex)
        {
            foreach(IReadOnlyList<Cell> list in ExtendedRunsOfFour)
            {
                if (list.Where(c => c.hex == hex).Count() > 0) return true;
            }
            return false;
        }

        private void IncrementTurn()
        {
            bool gameOver = (reserveBlackPieces == 0 || reserveWhitePieces == 0
                            || whiteGipfPiecesInPlay == 0 || (blackGipfPiecesInPlay == 0 && _turnNumber > 2));
            if (gameOver)
            {
                if (reserveBlackPieces == 0 || blackGipfPiecesInPlay == 0) _gameResult = GameResult.WhiteWin;
                else _gameResult = GameResult.BlackWin;
            }
            else
            {
                _gameResult = GameResult.Incomplete;
                _whiteToPlay = !_whiteToPlay;
                _turnNumber++;
            }
        }

        public bool TryGetPieceAtHex(Hex hex, out GipfPiece piece)
        {
            piece = _cellMap[hex].Piece;
            return (null != piece);
        }

        public Board Clone()
        {
            Board clonedBoard = Board.GetInitialBoard();
            foreach (var kvp in _cellMap.Where(kvp => kvp.Value.Piece != Pieces.NoPiece))
            {
                clonedBoard._cellMap[kvp.Key].SetPiece(kvp.Value.Piece);
            }
            return clonedBoard;
        }

        public static bool BoardPositionsEqual(Board x, Board y)
        {
            //if (x._reserveWhitePieces != y._reserveWhitePieces  ||
            //    x._reserveBlackPieces != y._reserveBlackPieces) return false;
            //foreach (var kvp in x.Cells)
            //{
            //    if (null == y._boardPieceArray[kvp.Value.column, kvp.Value.row]) return false;
            //    if (!y._boardPieceArray[kvp.Value.column, kvp.Value.row].Equals(kvp.Key)) return false;
            //}
            //return true;
            throw new NotImplementedException();
        }

        internal IEnumerable<Cell> GetWallsEnumerable()
        {
            return new BoardWallsEnumerable(this);
        }

        /// <summary>
        /// We create a board by getting a hex of hexes with radius of 4
        /// See http://www.redblobgames.com/grids/hexagons/#range
        /// </summary>
        public static Board GetInitialBoard()
        {
            Board board = new Board();
            for(int row = -boardRadius; row <= boardRadius; row++)
            {
                int minCol = Math.Max(-boardRadius, -row - boardRadius );
                int maxCol = Math.Min(boardRadius, -row + boardRadius);

                for(int col = minCol; col <= maxCol; col++)
                {
                    Hex hex = new Hex(col, row);
                    Cell cell;
                    if (Hex.Distance(start, hex) >= boardRadius)
                    {
                        cell = new Wall(board, hex);
                    }
                    else
                    {
                        cell = new Cell(board, hex);
                    }
                    board._cellMap[hex] = cell;
                }
            }
            board.InitCells();
            return board;
        }

        private void InitCells()
        {
            foreach (KeyValuePair<Hex, Cell> kvp in _cellMap)
            {
                for (int i = 1; i <= 6; i++)
                {
                    Cell ignore;
                    if (!kvp.Value.TryGetNeighbor((Position)i, out ignore))
                    {
                        Cell neighborCell;
                        if (_cellMap.TryGetValue(kvp.Key + Neighborhood.neighborDirections[i], out neighborCell))
                        {
                            Cell.LinkCells(kvp.Value, neighborCell, (Position)i);
                        }
                    }
                }
            }
        }
    }
}
