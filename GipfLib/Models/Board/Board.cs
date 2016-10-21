using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GipfLib.Models
{
    public enum GameType { Standard, Tournament }

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
        public PieceColor colorToPlay => _whiteToPlay ? PieceColor.White : PieceColor.Black;

        private GameResult _gameResult = GameResult.Incomplete;
        public GameResult gameResult => _gameResult;

        private int _turnNumber = 1;
        public int turnNumber => _turnNumber;

        private List<bool> _canPlayGipf = new List<bool>() { true, true };
        public bool WhiteCanPlayGipf => _canPlayGipf[(int)PieceColor.White];
        public bool BlackCanPlayGipf => _canPlayGipf[(int)PieceColor.Black];

        private GameType _gameType;
        public GameType gameType => _gameType;

        // end serialize

        private bool _mustPlayGipf => _gameType == GameType.Tournament && (_turnNumber == 1 || _turnNumber == 2);

        private bool _runsDirty = true;
        private bool _movesDirty = true;

        private List<Move> _moves = new List<Move>();

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
        internal IEnumerable<Cell> GetWallsEnumerable() => new BoardWallsEnumerable(this);

        private List<List<Hex>> _allPossibleRemoveLists = new List<List<Hex>>() { new List<Hex>() };
        public IReadOnlyList<IReadOnlyList<Hex>> AllPossibleRemoveBeforeLists => _allPossibleRemoveLists.AsReadOnly();

        private void CalculateAllPossibleRemoveLists()
        {
            _allPossibleRemoveLists.Clear();
            List<Hex> removeBefore = new List<Hex>();
            List<Hex> removeBeforeGipf = new List<Hex>();

            foreach (IReadOnlyList<Cell> extRun in _runsVisitor.ExtendedRunsOf4)
            {
                if (!(extRun[0].Piece.Color == colorToPlay)) throw new Exception("Prior to moving, there is a run of four that is not of the color whose turn it is");
                foreach (Cell c in extRun)
                {
                    // Gipf piece causes multiplication of moves due to choice
                    if (c.Piece.NumPieces == 2 && c.Piece.Color == colorToPlay)
                    {
                        removeBeforeGipf.Add(c.hex);
                    }
                    else
                    {
                        removeBefore.Add(c.hex);
                    }
                }
            }

            _allPossibleRemoveLists.Add(removeBefore);
            foreach (Hex hex in removeBeforeGipf)
            {
                List<List<Hex>> tempListList = new List<List<Hex>>();
                foreach (List<Hex> removeList in _allPossibleRemoveLists)
                {
                    List<Hex> tempList = new List<Hex>(removeList);
                    tempList.Add(hex);
                    tempListList.Add(tempList);
                }
                foreach (List<Hex> tempList in tempListList) _allPossibleRemoveLists.Add(tempList);
            }
        }

        // TODO fix generating duplicate moves ("b1-b2" == "a1-b2" when nothing is pushed)
        public IReadOnlyList<Move> GetMoves()
        {
            if (_movesDirty)
            {
                _moves.Clear();
                foreach (List<Hex> removeBeforeList in _allPossibleRemoveLists)
                {
                    Board prePushRemovedBoard = Clone();
                    prePushRemovedBoard.RemoveOrCapturePieces(removeBeforeList);
                    foreach (Wall wall in prePushRemovedBoard.GetWallsEnumerable())
                    {
                        foreach (Position pos in wall.PushPositions)
                        {
                            if (wall.CanPush(pos))
                            {
                                if (!_mustPlayGipf)
                                {
                                    Board pushBoard = prePushRemovedBoard.Clone();
                                    Wall pWall = (Wall)pushBoard.Cells[wall.hex];

                                    pWall.Push(pos, new GipfPiece(1, colorToPlay));
                                    pushBoard.CalculateAllPossibleRemoveLists();

                                    foreach (IReadOnlyList<Hex> removeAfterList in pushBoard.AllPossibleRemoveBeforeLists)
                                    {
                                        _moves.Add(new Move(pWall.hex, pWall.NeighborhoodCells[pos].hex, removeBeforeList.ToList(), removeAfterList.ToList(), false));
                                    }
                                }

                                if (_canPlayGipf[(int)colorToPlay])
                                {
                                    Board pushBoard = prePushRemovedBoard.Clone();
                                    Wall pWall = (Wall)pushBoard.Cells[wall.hex];

                                    pWall.Push(pos, new GipfPiece(2, colorToPlay));
                                    pushBoard.CalculateAllPossibleRemoveLists();

                                    foreach (IReadOnlyList<Hex> removeAfterList in pushBoard.AllPossibleRemoveBeforeLists)
                                    {
                                        _moves.Add(new Move(pWall.hex, pWall.NeighborhoodCells[pos].hex, removeBeforeList.ToList(), removeAfterList.ToList(), true));
                                    }

                                }
                            }
                        }

                    }
                }
                _movesDirty = false;
            }
            return _moves.AsReadOnly();
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
                if (false == _canPlayGipf[(int)colorToPlay] && move.isGipf) throw new Exception("Cannot play a Gipf piece at this time");

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

                if (!move.isGipf) _canPlayGipf[(int)colorToPlay] = false;
                _movesDirty = true;
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
                CalculateAllPossibleRemoveLists();
            }
        }

        public bool TryGetPieceAtHex(Hex hex, out GipfPiece piece)
        {
            piece = _cellMap[hex].Piece;
            return (null != piece);
        }

        public Board Clone()
        {
            Board clonedBoard = Board.GetInitialBoard(_gameType);
            foreach (var kvp in _cellMap)
            {
                clonedBoard._cellMap[kvp.Key].SetPiece(kvp.Value.Piece);
            }
            clonedBoard._capturedPieces = new List<int>(_capturedPieces);
            clonedBoard._reservePieces = new List<int>(_reservePieces);
            clonedBoard._gameResult = _gameResult;
            clonedBoard._movesDirty = _movesDirty;
            clonedBoard._moves = new List<Move>(_moves);
            clonedBoard._turnNumber = _turnNumber;
            clonedBoard._whiteToPlay = _whiteToPlay;
            clonedBoard._canPlayGipf = new List<bool>(_canPlayGipf);
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

 
        /// <summary>
        /// We create a board by getting a hex of hexes with radius of 4
        /// See http://www.redblobgames.com/grids/hexagons/#range
        /// </summary>
        public static Board GetInitialBoard(GameType gameType = GameType.Tournament)
        {
            Board board = new Board();
            board._gameType = gameType;
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
            if (gameType == GameType.Standard)
            {
                board._cellMap[NotationParser.GetHex("e8")].SetPiece(Pieces.BlackGipf);
                board._cellMap[NotationParser.GetHex("b2")].SetPiece(Pieces.BlackGipf);
                board._cellMap[NotationParser.GetHex("h2")].SetPiece(Pieces.BlackGipf);
                board._cellMap[NotationParser.GetHex("e2")].SetPiece(Pieces.WhiteGipf);
                board._cellMap[NotationParser.GetHex("b5")].SetPiece(Pieces.WhiteGipf);
                board._cellMap[NotationParser.GetHex("h5")].SetPiece(Pieces.WhiteGipf);
                board._canPlayGipf[(int)PieceColor.White] = false;
                board._canPlayGipf[(int)PieceColor.Black] = false;
            }
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
