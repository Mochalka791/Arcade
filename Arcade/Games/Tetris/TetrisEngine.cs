using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcade.Games.Tetris;

public enum TetrisTickResult
{
    None,
    PieceLocked,
    GameOver
}

public sealed class TetrisEngine
{
    public const int BoardWidth = 10;
    public const int BoardHeight = 20;

    private readonly TetrominoType?[,] _board = new TetrominoType?[BoardHeight, BoardWidth];
    private readonly Queue<TetrominoType> _nextQueue = new();
    private readonly Random _random;

    private TetrominoType? _currentType;
    private BoardPoint _currentPosition;
    private int _currentRotation;
    private TetrominoType? _holdType;
    private bool _holdUsed;
    private IReadOnlyList<int> _lastClearedRows = Array.Empty<int>();

    public TetrisEngine() : this(Random.Shared) { }

    public TetrisEngine(Random random)
    {
        _random = random;
        Reset();
    }

    public int Score { get; private set; }
    public int Level { get; private set; }
    public int LinesCleared { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    public TetrominoType? HoldPiece => _holdType;
    public TetrominoType? CurrentPiece => _currentType;
    public int CurrentRotation => _currentRotation;
    public BoardPoint CurrentPosition => _currentPosition;
    public IReadOnlyList<int> LastClearedRows => _lastClearedRows;

    public TimeSpan TickInterval => TimeSpan.FromMilliseconds(Math.Max(1000 - 60 * Level, 120));

    public IReadOnlyList<TetrominoType> NextPieces => _nextQueue.Take(5).ToList();

    public void Reset()
    {
        Array.Clear(_board, 0, _board.Length);
        _nextQueue.Clear();
        _currentType = null;
        _holdType = null;
        _holdUsed = false;
        _currentRotation = 0;
        _currentPosition = new BoardPoint(0, 0);
        Score = 0;
        Level = 0;
        LinesCleared = 0;
        IsPaused = false;
        IsGameOver = false;
        _lastClearedRows = Array.Empty<int>();

        EnsureNextQueue();
        SpawnFromQueue();
    }

    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;
    public void TogglePause()
    {
        if (IsGameOver) return;
        IsPaused = !IsPaused;
    }

    public TetrisTickResult Tick()
    {
        if (IsGameOver || _currentType is null)
        {
            return IsGameOver ? TetrisTickResult.GameOver : TetrisTickResult.None;
        }

        if (TryMove(new BoardPoint(1, 0)))
        {
            return TetrisTickResult.None;
        }

        return LockPiece();
    }

    public bool MoveLeft() => TryMove(new BoardPoint(0, -1));
    public bool MoveRight() => TryMove(new BoardPoint(0, 1));

    public bool SoftDrop()
    {
        if (IsGameOver || _currentType is null)
        {
            return false;
        }

        if (TryMove(new BoardPoint(1, 0)))
        {
            Score += 1;
            return true;
        }

        LockPiece();
        return false;
    }

    public int HardDrop()
    {
        if (IsGameOver || _currentType is null)
        {
            return 0;
        }

        var steps = 0;
        while (TryMove(new BoardPoint(1, 0)))
        {
            steps++;
        }

        if (steps > 0)
        {
            Score += steps * 2;
            LockPiece();
        }

        return steps;
    }

    public bool RotateClockwise() => TryRotate(1);
    public bool RotateCounterClockwise() => TryRotate(-1);

    public bool Hold()
    {
        if (_holdUsed || _currentType is null || IsGameOver)
        {
            return false;
        }

        var current = _currentType.Value;

        if (_holdType is null)
        {
            _holdType = current;
            SpawnFromQueue();
        }
        else
        {
            var swap = _holdType.Value;
            _holdType = current;
            SpawnCurrent(swap, resetHoldFlag: false);
        }

        _holdUsed = true;
        return true;
    }

    public TetrominoType? GetLockedCell(int row, int col) => _board[row, col];

    public IReadOnlyList<BoardPoint> GetCurrentCells()
    {
        if (_currentType is null)
        {
            return Array.Empty<BoardPoint>();
        }

        return GetCells(_currentType.Value, _currentRotation, _currentPosition);
    }

    public IReadOnlyList<BoardPoint> GetGhostCells()
    {
        if (_currentType is null || IsGameOver)
        {
            return Array.Empty<BoardPoint>();
        }

        var position = _currentPosition;
        while (IsPositionValid(_currentType.Value, _currentRotation, position + new BoardPoint(1, 0)))
        {
            position += new BoardPoint(1, 0);
        }

        return GetCells(_currentType.Value, _currentRotation, position);
    }

    private bool TryMove(BoardPoint delta)
    {
        if (_currentType is null || IsGameOver)
        {
            return false;
        }

        var target = _currentPosition + delta;
        if (!IsPositionValid(_currentType.Value, _currentRotation, target))
        {
            return false;
        }

        _currentPosition = target;
        return true;
    }

    private bool TryRotate(int delta)
    {
        if (_currentType is null || IsGameOver)
        {
            return false;
        }

        var from = _currentRotation;
        var to = Tetromino.NormalizeRotation(_currentRotation + delta);
        var type = _currentType.Value;

        foreach (var offset in Tetrominoes.GetKickOffsets(type, from, to))
        {
            var targetPosition = new BoardPoint(_currentPosition.Row + offset.Row, _currentPosition.Col + offset.Col);
            if (IsPositionValid(type, to, targetPosition))
            {
                _currentRotation = to;
                _currentPosition = targetPosition;
                return true;
            }
        }

        return false;
    }

    private bool IsPositionValid(TetrominoType type, int rotation, BoardPoint position)
    {
        foreach (var cell in GetCells(type, rotation, position))
        {
            if (cell.Col < 0 || cell.Col >= BoardWidth)
            {
                return false;
            }

            if (cell.Row >= BoardHeight)
            {
                return false;
            }

            if (cell.Row >= 0 && _board[cell.Row, cell.Col] is not null)
            {
                return false;
            }
        }

        return true;
    }

    private TetrisTickResult LockPiece()
    {
        if (_currentType is null)
        {
            return TetrisTickResult.None;
        }

        var type = _currentType.Value;
        foreach (var cell in GetCells(type, _currentRotation, _currentPosition))
        {
            if (cell.Row < 0)
            {
                IsGameOver = true;
                continue;
            }

            _board[cell.Row, cell.Col] = type;
        }

        var cleared = ClearLines();
        ApplyLineClearScore(cleared);

        if (!IsGameOver)
        {
            SpawnFromQueue();
        }

        return IsGameOver ? TetrisTickResult.GameOver : TetrisTickResult.PieceLocked;
    }

    private int ClearLines()
    {
        var cleared = 0;
        var clearedRows = new List<int>();

        for (var row = BoardHeight - 1; row >= 0; row--)
        {
            var full = true;
            for (var col = 0; col < BoardWidth; col++)
            {
                if (_board[row, col] is null)
                {
                    full = false;
                    break;
                }
            }

            if (!full)
            {
                continue;
            }

            cleared++;
            clearedRows.Add(row);

            for (var r = row; r > 0; r--)
            {
                for (var c = 0; c < BoardWidth; c++)
                {
                    _board[r, c] = _board[r - 1, c];
                }
            }

            for (var c = 0; c < BoardWidth; c++)
            {
                _board[0, c] = null;
            }

            row++;
        }

        _lastClearedRows = clearedRows.Count > 0 ? clearedRows : Array.Empty<int>();

        return cleared;
    }

    private void ApplyLineClearScore(int cleared)
    {
        if (cleared <= 0)
        {
            return;
        }

        Score += cleared switch
        {
            1 => 100,
            2 => 300,
            3 => 500,
            4 => 800,
            _ => 0
        };

        LinesCleared += cleared;
        UpdateLevel();
    }

    private void UpdateLevel()
    {
        Level = LinesCleared / 10;
    }

    private void SpawnFromQueue()
    {
        EnsureNextQueue();
        var next = _nextQueue.Dequeue();
        SpawnCurrent(next, resetHoldFlag: true);
    }

    private void SpawnCurrent(TetrominoType type, bool resetHoldFlag)
    {
        _currentType = type;
        _currentRotation = 0;
        _currentPosition = new BoardPoint(-2, 3);
        if (resetHoldFlag)
        {
            _holdUsed = false;
        }

        if (!IsPositionValid(type, _currentRotation, _currentPosition))
        {
            IsGameOver = true;
        }
    }

    private void EnsureNextQueue()
    {
        if (_nextQueue.Count >= 7)
        {
            return;
        }

        var bag = Enum.GetValues<TetrominoType>().ToList();
        Shuffle(bag, _random);
        foreach (var piece in bag)
        {
            _nextQueue.Enqueue(piece);
        }
    }

    private static void Shuffle(IList<TetrominoType> list, Random random)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static IReadOnlyList<BoardPoint> GetCells(TetrominoType type, int rotation, BoardPoint origin)
    {
        var tetromino = Tetrominoes.Get(type);
        var offsets = tetromino.GetRotation(rotation);
        var result = new BoardPoint[offsets.Length];
        for (var i = 0; i < offsets.Length; i++)
        {
            result[i] = new BoardPoint(origin.Row + offsets[i].Row, origin.Col + offsets[i].Col);
        }

        return result;
    }

    public void ResetLastClearedRows()
    {
        _lastClearedRows = Array.Empty<int>();
    }
}
