using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcade.Games.Tetris;

public sealed class TetrisEngine
{
    public const int BoardWidth = 10;
    public const int BoardHeight = 20;

    private readonly TetrominoType?[,] _lockedCells = new TetrominoType?[BoardHeight, BoardWidth];
    private readonly List<TetrominoType> _nextPieces = new();
    private readonly List<int> _lastClearedRows = new();
    private readonly Random _random = new();

    private BoardPoint _currentPosition;
    private int _currentRotation;
    private bool _canHold = true;
    private List<TetrominoType> _bag = new();

    // Bot state
    private int? _botTargetCol;
    private int? _botTargetRotation;
    private bool _botMovesCalculated;

    public int Score { get; private set; }
    public int Level { get; private set; } = 1;
    public int LinesCleared { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public bool AutoPlay { get; set; }

    public TetrominoType? CurrentPiece { get; private set; }
    public TetrominoType? HoldPiece { get; private set; }
    public IReadOnlyList<TetrominoType> NextPieces => _nextPieces;
    public IReadOnlyList<int> LastClearedRows => _lastClearedRows;

    public TimeSpan TickInterval => TimeSpan.FromMilliseconds(AutoPlay ? 30 : Math.Max(50, 1000 - Level * 50));

    public TetrisEngine() => Reset();

    public void TogglePause()
    {
        if (!IsGameOver)
            IsPaused = !IsPaused;
    }

    public void Reset()
    {
        Score = 0;
        Level = 1;
        LinesCleared = 0;
        IsGameOver = false;
        IsPaused = false;
        AutoPlay = false;

        _canHold = true;
        CurrentPiece = null;
        HoldPiece = null;
        _lastClearedRows.Clear();
        _botMovesCalculated = false;

        for (var r = 0; r < BoardHeight; r++)
            for (var c = 0; c < BoardWidth; c++)
                _lockedCells[r, c] = null;

        _nextPieces.Clear();
        _bag.Clear();
        FillNextPieces();
        SpawnNewPiece();
    }

    public void Tick()
    {
        if (IsGameOver || IsPaused || CurrentPiece is null) return;

        if (AutoPlay)
        {
            if (!_botMovesCalculated)
            {
                CalculateBotMove();
                _botMovesCalculated = true;
            }

            if (ApplyBotMove())
                return; // Bot ist noch am Positionieren

            // Bot ist fertig positioniert -> HardDrop
            HardDrop();
            return;
        }

        var newPos = _currentPosition with { Row = _currentPosition.Row + 1 };

        if (IsValidPosition(CurrentPiece.Value, newPos, _currentRotation))
        {
            _currentPosition = newPos;
            return;
        }

        LockPiece();
        var lines = ClearLines();
        UpdateScore(lines);
        SpawnNewPiece();
    }

    public void MoveLeft()
    {
        if (IsPaused || IsGameOver || CurrentPiece is null) return;
        var newPos = _currentPosition with { Col = _currentPosition.Col - 1 };
        if (IsValidPosition(CurrentPiece.Value, newPos, _currentRotation))
            _currentPosition = newPos;
    }

    public void MoveRight()
    {
        if (IsPaused || IsGameOver || CurrentPiece is null) return;
        var newPos = _currentPosition with { Col = _currentPosition.Col + 1 };
        if (IsValidPosition(CurrentPiece.Value, newPos, _currentRotation))
            _currentPosition = newPos;
    }

    public void SoftDrop()
    {
        if (IsPaused || IsGameOver) return;
        Tick();
    }

    public void RotateClockwise()
    {
        if (IsPaused || IsGameOver || CurrentPiece is null) return;
        var newRot = (_currentRotation + 1) % 4;
        if (TryRotation(newRot, 0)) return;
        if (TryRotation(newRot, 1)) return;
        if (TryRotation(newRot, -1)) return;
    }

    public void RotateCounterClockwise()
    {
        if (IsPaused || IsGameOver || CurrentPiece is null) return;
        var newRot = (_currentRotation + 3) % 4;
        if (TryRotation(newRot, 0)) return;
        if (TryRotation(newRot, 1)) return;
        if (TryRotation(newRot, -1)) return;
    }

    public void HardDrop()
    {
        if (IsPaused || IsGameOver || CurrentPiece is null) return;

        var pos = _currentPosition;
        while (IsValidPosition(CurrentPiece.Value, pos with { Row = pos.Row + 1 }, _currentRotation))
            pos = pos with { Row = pos.Row + 1 };

        _currentPosition = pos;

        LockPiece();
        var lines = ClearLines();
        UpdateScore(lines);
        SpawnNewPiece();
    }

    public void Hold()
    {
        if (IsPaused || IsGameOver || !_canHold || CurrentPiece is null) return;

        _canHold = false;
        var pieceToHold = CurrentPiece.Value;

        if (HoldPiece is null)
        {
            HoldPiece = pieceToHold;
            SpawnNewPiece();
            return;
        }

        var newCurrent = HoldPiece.Value;
        HoldPiece = pieceToHold;
        CurrentPiece = newCurrent;
        ResetCurrentPiecePosition();
    }

    private void SpawnNewPiece()
    {
        if (_nextPieces.Count == 0)
            FillNextPieces();

        CurrentPiece = _nextPieces[0];
        _nextPieces.RemoveAt(0);
        FillNextPieces();

        ResetCurrentPiecePosition();
        _canHold = true;
        _botMovesCalculated = false;

        if (!IsValidPosition(CurrentPiece.Value, _currentPosition, _currentRotation))
        {
            IsGameOver = true;
            CurrentPiece = null;
        }
    }

    private void ResetCurrentPiecePosition()
    {
        _currentRotation = 0;
        _currentPosition = new BoardPoint(0, BoardWidth / 2 - 2);

        if (CurrentPiece is TetrominoType.T or TetrominoType.O)
            _currentPosition = _currentPosition with { Row = -1 };
    }

    private void FillNextPieces()
    {
        while (_nextPieces.Count < 5)
        {
            if (_bag.Count == 0)
                _bag = Enum.GetValues<TetrominoType>().OrderBy(_ => _random.Next()).ToList();

            _nextPieces.Add(_bag[0]);
            _bag.RemoveAt(0);
        }
    }

    private bool IsValidPosition(TetrominoType type, BoardPoint position, int rotation)
    {
        var tetromino = Tetrominoes.Get(type);
        var cells = tetromino.GetRotation(rotation);

        foreach (var cell in cells)
        {
            var r = cell.Row + position.Row;
            var c = cell.Col + position.Col;

            if (c < 0 || c >= BoardWidth || r >= BoardHeight)
                return false;

            if (r < 0)
                continue;

            if (_lockedCells[r, c] is not null)
                return false;
        }
        return true;
    }

    private bool TryRotation(int newRot, int colOffset)
    {
        if (CurrentPiece is null) return false;

        var newPos = _currentPosition with { Col = _currentPosition.Col + colOffset };
        if (IsValidPosition(CurrentPiece.Value, newPos, newRot))
        {
            _currentRotation = newRot;
            _currentPosition = newPos;
            return true;
        }
        return false;
    }

    private void LockPiece()
    {
        if (CurrentPiece is null) return;

        foreach (var cell in GetCurrentCells())
        {
            if (cell.Row >= 0 && cell.Row < BoardHeight && cell.Col >= 0 && cell.Col < BoardWidth)
                _lockedCells[cell.Row, cell.Col] = CurrentPiece.Value;
        }
        CurrentPiece = null;
    }

    private int ClearLines()
    {
        _lastClearedRows.Clear();
        var lines = 0;

        for (var r = BoardHeight - 1; r >= 0; r--)
        {
            var isFull = true;
            for (var c = 0; c < BoardWidth; c++)
            {
                if (_lockedCells[r, c] is null) { isFull = false; break; }
            }

            if (!isFull) continue;

            _lastClearedRows.Add(r);
            lines++;

            for (var row = r; row > 0; row--)
                for (var c = 0; c < BoardWidth; c++)
                    _lockedCells[row, c] = _lockedCells[row - 1, c];

            for (var c = 0; c < BoardWidth; c++)
                _lockedCells[0, c] = null;

            r++;
        }
        return lines;
    }

    private void UpdateScore(int lines)
    {
        if (lines == 0) return;

        LinesCleared += lines;
        var points = lines switch
        {
            1 => 100 * Level,
            2 => 300 * Level,
            3 => 500 * Level,
            4 => 800 * Level,
            _ => 0
        };
        Score += points;
        Level = LinesCleared / 10 + 1;
    }

    // =======================================================
    // INTELLIGENTER BOT
    // =======================================================

    private void CalculateBotMove()
    {
        if (CurrentPiece is null) return;

        int bestCol = 0;
        int bestRotation = 0;
        double bestScore = double.MinValue;

        // Teste alle möglichen Positionen und Rotationen
        for (int rot = 0; rot < 4; rot++)
        {
            for (int col = -2; col < BoardWidth + 2; col++)
            {
                var testPos = new BoardPoint(0, col);

                if (!IsValidPosition(CurrentPiece.Value, testPos, rot))
                    continue;

                // Finde die Landing-Position
                var landingPos = testPos;
                while (IsValidPosition(CurrentPiece.Value, landingPos with { Row = landingPos.Row + 1 }, rot))
                    landingPos = landingPos with { Row = landingPos.Row + 1 };

                // Bewerte diese Position
                double score = EvaluatePosition(landingPos, rot);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCol = col;
                    bestRotation = rot;
                }
            }
        }

        _botTargetCol = bestCol;
        _botTargetRotation = bestRotation;
    }

    private double EvaluatePosition(BoardPoint position, int rotation)
    {
        if (CurrentPiece is null) return double.MinValue;

        // Simuliere das Platzieren
        var testBoard = (TetrominoType?[,])_lockedCells.Clone();
        var cells = Tetrominoes.Get(CurrentPiece.Value).GetRotation(rotation);

        foreach (var cell in cells)
        {
            int r = cell.Row + position.Row;
            int c = cell.Col + position.Col;
            if (r >= 0 && r < BoardHeight && c >= 0 && c < BoardWidth)
                testBoard[r, c] = CurrentPiece.Value;
        }

        double score = 0;

        // 1. Anzahl gelöschter Zeilen (sehr wichtig)
        int completedLines = CountCompletedLines(testBoard);
        score += completedLines * 100;

        // 2. Aggregierte Höhe (niedriger = besser)
        int aggregateHeight = 0;
        for (int c = 0; c < BoardWidth; c++)
            aggregateHeight += GetColumnHeightInBoard(testBoard, c);
        score -= aggregateHeight * 0.5;

        // 3. Löcher (weniger = besser)
        int holes = CountHoles(testBoard);
        score -= holes * 50;

        // 4. Unebenheit (weniger = besser)
        int bumpiness = CalculateBumpiness(testBoard);
        score -= bumpiness * 2;

        // 5. Berühre Wände/Boden für Stabilität
        bool touchesWall = cells.Any(cell =>
        {
            int c = cell.Col + position.Col;
            return c == 0 || c == BoardWidth - 1;
        });
        if (touchesWall) score += 1;

        return score;
    }

    private int CountCompletedLines(TetrominoType?[,] board)
    {
        int count = 0;
        for (int r = 0; r < BoardHeight; r++)
        {
            bool isFull = true;
            for (int c = 0; c < BoardWidth; c++)
            {
                if (board[r, c] is null)
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull) count++;
        }
        return count;
    }

    private int GetColumnHeightInBoard(TetrominoType?[,] board, int col)
    {
        for (int r = 0; r < BoardHeight; r++)
        {
            if (board[r, col] is not null)
                return BoardHeight - r;
        }
        return 0;
    }

    private int CountHoles(TetrominoType?[,] board)
    {
        int holes = 0;
        for (int c = 0; c < BoardWidth; c++)
        {
            bool foundBlock = false;
            for (int r = 0; r < BoardHeight; r++)
            {
                if (board[r, c] is not null)
                    foundBlock = true;
                else if (foundBlock)
                    holes++;
            }
        }
        return holes;
    }

    private int CalculateBumpiness(TetrominoType?[,] board)
    {
        int bumpiness = 0;
        for (int c = 0; c < BoardWidth - 1; c++)
        {
            int h1 = GetColumnHeightInBoard(board, c);
            int h2 = GetColumnHeightInBoard(board, c + 1);
            bumpiness += Math.Abs(h1 - h2);
        }
        return bumpiness;
    }

    private bool ApplyBotMove()
    {
        if (CurrentPiece is null || _botTargetCol is null || _botTargetRotation is null)
            return false;

        // Erst zur Zielrotation rotieren
        if (_currentRotation != _botTargetRotation.Value)
        {
            var newRot = (_currentRotation + 1) % 4;
            if (TryRotation(newRot, 0) || TryRotation(newRot, 1) || TryRotation(newRot, -1))
                return true;
        }

        // Dann zur Zielspalte bewegen
        if (_currentPosition.Col < _botTargetCol.Value)
        {
            MoveRight();
            return true;
        }
        else if (_currentPosition.Col > _botTargetCol.Value)
        {
            MoveLeft();
            return true;
        }

        // Zielposition erreicht
        return false;
    }

    // =======================================================

    public IReadOnlyList<BoardPoint> GetCurrentCells()
    {
        if (CurrentPiece is not TetrominoType type)
            return Array.Empty<BoardPoint>();

        var baseCells = Tetrominoes.Get(type).GetRotation(_currentRotation);
        return baseCells.Select(c => c with
        {
            Row = c.Row + _currentPosition.Row,
            Col = c.Col + _currentPosition.Col
        }).ToList();
    }

    public IReadOnlyList<BoardPoint> GetGhostCells()
    {
        if (CurrentPiece is not TetrominoType type)
            return Array.Empty<BoardPoint>();

        var ghost = _currentPosition;
        while (IsValidPosition(type, ghost with { Row = ghost.Row + 1 }, _currentRotation))
            ghost = ghost with { Row = ghost.Row + 1 };

        var baseCells = Tetrominoes.Get(type).GetRotation(_currentRotation);
        return baseCells.Select(c => c with
        {
            Row = c.Row + ghost.Row,
            Col = c.Col + ghost.Col
        }).ToList();
    }

    public TetrominoType? GetLockedCell(int row, int col)
    {
        if (row < 0 || row >= BoardHeight || col < 0 || col >= BoardWidth)
            return null;
        return _lockedCells[row, col];
    }

    public void ResetLastClearedRows() => _lastClearedRows.Clear();
}