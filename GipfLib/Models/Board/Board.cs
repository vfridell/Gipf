using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GipfLib.Models
{
    public enum GameType { Standard, Tournament }

    public class Board
    {
        private static readonly Hex Start = new Hex(0, 0);
        private static readonly int BoardRadius = 4;

        // serialize this stuff
        Dictionary<Hex, Cell> _cellMap = new Dictionary<Hex, Cell>();

        private List<int> _reservePieces = new List<int>(){ 18, 18 };
        private List<int> _capturedPieces = new List<int>(){ 0, 0 };
        public int ReserveWhitePieces => _reservePieces[(int)PieceColor.White];
        public int ReserveBlackPieces => _reservePieces[(int)PieceColor.Black];

        private bool _whiteToPlay = true;
        public bool WhiteToPlay => _whiteToPlay;

        public PieceColor ColorToPlay => _whiteToPlay ? PieceColor.White : PieceColor.Black;

        private GameResult _gameResult = GameResult.Incomplete;
        public GameResult GameResult => _gameResult;

        private int _turnNumber = 1;
        public int TurnNumber => _turnNumber;

        private List<bool> _canPlayGipf = new List<bool>() { true, true };
        public bool WhiteCanPlayGipf => _canPlayGipf[(int)PieceColor.White];
        public bool BlackCanPlayGipf => _canPlayGipf[(int)PieceColor.Black];

        private GameType _gameType;
        public GameType GameType => _gameType;

        // end serialize

        private bool MustPlayGipf => _gameType == GameType.Tournament && (_turnNumber == 1 || _turnNumber == 2);

        private bool _runsDirty = true;
        private bool _movesDirty = true;

        private HashSet<Move> _moves = new HashSet<Move>();

        private PieceCountVisitor _gipfCountVisitor = new PieceCountVisitor();
        public int BlackGipfPiecesInPlay 
        { get { if (_runsDirty) FindRuns(); return _gipfCountVisitor.BlackGipfCount; } }
        public int WhiteGipfPiecesInPlay 
        { get { if (_runsDirty) FindRuns(); return _gipfCountVisitor.WhiteGipfCount; } }
        public int WhitePiecesInPlay 
        { get { if (_runsDirty) FindRuns(); return _gipfCountVisitor.WhiteNonGipfCount; } }
        public int BlackPiecesInPlay 
        { get { if (_runsDirty) FindRuns(); return _gipfCountVisitor.BlackNonGipfCount; } }

        private FindRunsVisitor _runsVisitor = new FindRunsVisitor();
        public IReadOnlyList<CellRun> Runs
        { get { if (_runsDirty) FindRuns(); return _runsVisitor.Runs; } }

        public IReadOnlyList<CellRun> RunsOfFour 
        { get { if (_runsDirty) FindRuns(); return _runsVisitor.RunsOfFourOrMore; } }

        public IReadOnlyList<CellRun> ExtendedRunsOfFour 
        { get { if (_runsDirty) FindRuns(); return _runsVisitor.ExtendedRuns; } }

        public string LastError => _lastError;
        private string _lastError;

        private void AddToReserve(PieceColor color, int amount) => _reservePieces[(int)color] += amount;
        private void RemoveFromReserve(PieceColor color, int amount) => _reservePieces[(int)color] -= amount;
        private void AddToCaptured(PieceColor color, int amount) => _capturedPieces[(int)color] += amount;

        private Board() { }

        public IReadOnlyDictionary<Hex, Cell> Cells => new ReadOnlyDictionary<Hex, Cell>(_cellMap);

        public IEnumerable<Cell> GetLinesEnumerable() => new BoardLinesEnumerable(this);
        internal IEnumerable<Cell> GetWallsEnumerable() => new BoardWallsEnumerable(this);

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private List<RemoveMovePart> _allPossibleRemoveLists = new List<RemoveMovePart>();
        public IReadOnlyList<RemoveMovePart> AllPossibleRemoveLists => _allPossibleRemoveLists.AsReadOnly();

        // TODO Fix this function with CellRun 
        private void CalculateAllPossibleRemoveLists()
        {
            _allPossibleRemoveLists.Clear();
            List<Hex> removeBefore = new List<Hex>();
            List<Hex> removeBeforeGipf = new List<Hex>();

            foreach (CellRun extRun in _runsVisitor.ExtendedRuns)
            {
                // TODO does this check apply to all cases where we run this function?
                // if the run of four is all gipf, do not throw an exception
                if (extRun.Color != ColorToPlay && !extRun.AllGipf) throw new Exception("Prior to moving, there is a run of four that is not of the color whose turn it is");
                foreach (Cell c in extRun.Run)
                {
                    // Gipf piece causes multiplication of moves due to choice
                    if (c.Piece.NumPieces == 2)
                    {
                        removeBeforeGipf.Add(c.hex);
                    }
                    else
                    {
                        removeBefore.Add(c.hex);
                    }
                }
            }

            _allPossibleRemoveLists.Add(new RemoveMovePart(removeBefore));
            foreach (Hex hex in removeBeforeGipf)
            {
                List<List<Hex>> tempListList = new List<List<Hex>>();
                foreach (RemoveMovePart removePart in _allPossibleRemoveLists)
                {
                    List<Hex> tempList = new List<Hex>(removePart.HexesToRemove);
                    tempList.Add(hex);
                    tempListList.Add(tempList);
                }
                foreach (List<Hex> tempList in tempListList) _allPossibleRemoveLists.Add(new RemoveMovePart(tempList));
            }
        }

        public IReadOnlyList<Move> GetMoves()
        {
            if (_movesDirty)
            {
                _moves.Clear();
                foreach (RemoveMovePart removeBeforeList in _allPossibleRemoveLists)
                {
                    Board prePushRemovedBoard = Clone();
                    prePushRemovedBoard.RemoveOrCapturePieces(removeBeforeList.HexesToRemove);
                    // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                    foreach (Wall wall in prePushRemovedBoard.GetWallsEnumerable())
                    {
                        foreach (Position pos in wall.PushPositions)
                        {
                            if (wall.CanPush(pos))
                            {
                                if (!MustPlayGipf)
                                {
                                    AddPossibleMoves(removeBeforeList.HexesToRemove, prePushRemovedBoard, wall.hex, pos, false);
                                }

                                if (_canPlayGipf[(int)ColorToPlay])
                                {
                                    AddPossibleMoves(removeBeforeList.HexesToRemove, prePushRemovedBoard, wall.hex, pos, true);
                                }
                            }
                        }

                    }
                }
                _movesDirty = false;
            }
            return _moves.ToList().AsReadOnly();
        }

        private void AddPossibleMoves(IReadOnlyList<Hex> removeBeforeList, Board prePushRemovedBoard, Hex wallHex, Position pos, bool isGipf)
        { throw new NotImplementedException(); }
/*
 *        private void AddPossibleMoves(List<Hex> removeBeforeList, Board prePushRemovedBoard, Hex wallHex, Position pos, bool isGipf)
        {
            Board pushBoard = prePushRemovedBoard.Clone();
            Wall pWall = (Wall)pushBoard.Cells[wallHex];

            pWall.Push(pos, new GipfPiece(isGipf ? 2 : 1, colorToPlay));
            pushBoard.FindRuns();
            pushBoard.CalculateAllPossibleRemoveLists();

            foreach (IReadOnlyList<Hex> removeAfterList in pushBoard.AllPossibleRemoveLists)
            {
                Move move = new Move(pWall.hex, pWall.NeighborhoodCells[pos].hex, removeBeforeList.ToList(), removeAfterList.ToList(), isGipf);
                move.SimplifyMove(prePushRemovedBoard);
                _moves.Add(move);
            }
        }
        */
        /// <summary>
        /// Make a move.  Validates the move or fails.
        /// </summary>
        /// <param name="move"></param>
        public bool TryMakeMove(Move move)
        {
            try
            {
                if (_gameResult != GameResult.Incomplete) throw new Exception("This game is over");
                if (false == _canPlayGipf[(int)ColorToPlay] && move.isGipf) throw new Exception("Cannot play a Gipf piece at this time");

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

                // TODO if the run of four is all gipf, do not throw an exception
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

                if (ExtendedRunsOfFour.Count(r => r.Color == ColorToPlay) != 0) throw new Exception($"Post-push removal did not clear all extended runs of four of current player's color ({ColorToPlay})");

                if (!move.isGipf) _canPlayGipf[(int)ColorToPlay] = false;
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
                if (pieceToRemove.Color == ColorToPlay)
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
            return ExtendedRunsOfFour.Any(run => run.Run.Count(c => c.hex == hex) > 0);
        }

        private void IncrementTurn()
        {
            // TODO fix the case when there are no pieces in reserve at end of turn but next turn there is some for removal.
            bool gameOver = (ReserveBlackPieces == 0 || ReserveWhitePieces == 0
                            || WhiteGipfPiecesInPlay == 0 || (BlackGipfPiecesInPlay == 0 && _turnNumber > 2));
            if (gameOver)
            {
                if (ReserveBlackPieces == 0 || BlackGipfPiecesInPlay == 0) _gameResult = GameResult.WhiteWin;
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
            if (!_cellMap.ContainsKey(hex))
            {
                piece = Pieces.NoPiece;
                return false;
            }
            else
            {
                piece = _cellMap[hex].Piece;
                return true;
            }
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
            clonedBoard._moves = new HashSet<Move>(_moves);
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
 
        public static bool IsCongruent(Board board1, Board board2)
        {
            if (!EqualPiecesInPlay(board1, board2)) return false;

            // check reflection congruence
            for (int i = 1; i <= 6; i++)
            {
                bool isCongruent = true;
                foreach (Cell cell in board1._cellMap.Values)
                {
                    if (cell.Piece != board2._cellMap[Neighborhood.MirrorOriginHex(cell.hex, i)].Piece)
                    {
                        isCongruent = false;
                        break;
                    }
                }
                if (isCongruent) return true;
            }

            // check rotational congruence
            for (int i = 1; i < 6; i++)
            {
                bool isCongruent = true;
                foreach(Cell cell in board1._cellMap.Values)
                {
                    if(cell.Piece != board2._cellMap[Neighborhood.Rotate60DegreesClockwiseHex(cell.hex, i)].Piece)
                    {
                        isCongruent = false;
                        break;
                    }
                }
                if (isCongruent) return true;
            }
            return false;
        }

        public static bool EqualPiecesInPlay(Board board1, Board board2)
        {
            return board1.BlackGipfPiecesInPlay == board2.BlackGipfPiecesInPlay &&
                    board1.WhiteGipfPiecesInPlay == board2.WhiteGipfPiecesInPlay &&
                    board1.BlackPiecesInPlay == board2.BlackPiecesInPlay &&
                    board1.WhitePiecesInPlay == board2.WhitePiecesInPlay;
        }

        /// <summary>
        /// We create a board by getting a hex of hexes with radius of 4
        /// See http://www.redblobgames.com/grids/hexagons/#range
        /// </summary>
        public static Board GetInitialBoard(GameType gameType = GameType.Tournament)
        {
            Board board = new Board {_gameType = gameType};
            for(int row = -BoardRadius; row <= BoardRadius; row++)
            {
                int minCol = Math.Max(-BoardRadius, -row - BoardRadius );
                int maxCol = Math.Min(BoardRadius, -row + BoardRadius);

                for(int col = minCol; col <= maxCol; col++)
                {
                    Hex hex = new Hex(col, row);
                    Cell cell;
                    if (Hex.Distance(Start, hex) >= BoardRadius)
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
