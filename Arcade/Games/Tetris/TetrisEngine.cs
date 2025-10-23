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

    public int Score { get; private set; }
    public int Level { get; private set; } = 1;
    public int LinesCleared { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    public TetrominoType? CurrentPiece { get; private set; }
    public TetrominoType? HoldPiece { get; private set; }
    public IReadOnlyList<TetrominoType> NextPieces => _nextPieces;
    public IReadOnlyList<int> LastClearedRows => _lastClearedRows;

    public TimeSpan TickInterval => TimeSpan.FromMilliseconds(Math.Max(50, 1000 - Level * 50));

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
        _canHold = true;
        CurrentPiece = null;
        HoldPiece = null;
        _lastClearedRows.Clear();

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

    // ---------- Helpers ----------

    private void SpawnNewPiece()
    {
        if (_nextPieces.Count == 0)
            FillNextPieces();

        CurrentPiece = _nextPieces[0];
        _nextPieces.RemoveAt(0);
        FillNextPieces();

        ResetCurrentPiecePosition();
        _canHold = true;

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

            r++; // dieselbe Zeile erneut prüfen (nach Down-Shift)
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

    // ---------- Rendering helpers ----------

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
