using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Arcade.Games.Tetris;

public enum TetrominoType
{
    I,
    O,
    T,
    S,
    Z,
    J,
    L
}

public readonly record struct BoardPoint(int Row, int Col)
{
    public static BoardPoint operator +(BoardPoint left, BoardPoint right) => new(left.Row + right.Row, left.Col + right.Col);
}

public sealed class Tetromino
{
    public TetrominoType Type { get; }
    public string CssClass { get; }
    public IReadOnlyList<BoardPoint[]> Rotations { get; }

    internal Tetromino(TetrominoType type, string cssClass, BoardPoint[][] rotations)
    {
        Type = type;
        CssClass = cssClass;
        Rotations = new ReadOnlyCollection<BoardPoint[]>(rotations);
    }

    public BoardPoint[] GetRotation(int rotation) => Rotations[Tetromino.NormalizeRotation(rotation)];

    public static int NormalizeRotation(int rotation)
    {
        var normalized = rotation % 4;
        return normalized < 0 ? normalized + 4 : normalized;
    }
}

public static class Tetrominoes
{
    private static readonly IReadOnlyDictionary<TetrominoType, Tetromino> _tetrominoes = BuildTetrominoes();
    private static readonly IReadOnlyDictionary<(TetrominoType, int, int), BoardPoint[]> _kickTable = BuildKickTable();
    private static readonly IReadOnlyList<Tetromino> _all = new ReadOnlyCollection<Tetromino>(
        Enum.GetValues<TetrominoType>().Select(t => _tetrominoes[t]).ToList());
    private static readonly BoardPoint[] _defaultKick = { new(0, 0) };

    public static IReadOnlyList<Tetromino> All => _all;

    public static Tetromino Get(TetrominoType type) => _tetrominoes[type];

    public static IReadOnlyList<BoardPoint> GetKickOffsets(TetrominoType type, int fromRotation, int toRotation)
    {
        var from = Tetromino.NormalizeRotation(fromRotation);
        var to = Tetromino.NormalizeRotation(toRotation);

        return _kickTable.TryGetValue((type, from, to), out var offsets)
            ? offsets
            : _defaultKick;
    }

    private static IReadOnlyDictionary<TetrominoType, Tetromino> BuildTetrominoes()
    {
        var map = new Dictionary<TetrominoType, Tetromino>
        {
            {
                TetrominoType.I,
                new Tetromino(
                    TetrominoType.I,
                    "i",
                    new[]
                    {
                        new[] { new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(1, 2), new BoardPoint(1, 3) },
                        new[] { new BoardPoint(0, 2), new BoardPoint(1, 2), new BoardPoint(2, 2), new BoardPoint(3, 2) },
                        new[] { new BoardPoint(2, 0), new BoardPoint(2, 1), new BoardPoint(2, 2), new BoardPoint(2, 3) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(1, 1), new BoardPoint(2, 1), new BoardPoint(3, 1) }
                    })
            },
            {
                TetrominoType.O,
                new Tetromino(
                    TetrominoType.O,
                    "o",
                    new[]
                    {
                        new[] { new BoardPoint(0, 1), new BoardPoint(0, 2), new BoardPoint(1, 1), new BoardPoint(1, 2) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(0, 2), new BoardPoint(1, 1), new BoardPoint(1, 2) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(0, 2), new BoardPoint(1, 1), new BoardPoint(1, 2) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(0, 2), new BoardPoint(1, 1), new BoardPoint(1, 2) }
                    })
            },
            {
                TetrominoType.T,
                new Tetromino(
                    TetrominoType.T,
                    "t",
                    new[]
                    {
                        new[] { new BoardPoint(0, 1), new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(1, 2) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(1, 1), new BoardPoint(1, 2), new BoardPoint(2, 1) },
                        new[] { new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(1, 2), new BoardPoint(2, 1) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(2, 1) }
                    })
            },
            {
                TetrominoType.S,
                new Tetromino(
                    TetrominoType.S,
                    "s",
                    new[]
                    {
                        new[] { new BoardPoint(0, 1), new BoardPoint(0, 2), new BoardPoint(1, 0), new BoardPoint(1, 1) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(1, 1), new BoardPoint(1, 2), new BoardPoint(2, 2) },
                        new[] { new BoardPoint(1, 1), new BoardPoint(1, 2), new BoardPoint(2, 0), new BoardPoint(2, 1) },
                        new[] { new BoardPoint(0, 0), new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(2, 1) }
                    })
            },
            {
                TetrominoType.Z,
                new Tetromino(
                    TetrominoType.Z,
                    "z",
                    new[]
                    {
                        new[] { new BoardPoint(0, 0), new BoardPoint(0, 1), new BoardPoint(1, 1), new BoardPoint(1, 2) },
                        new[] { new BoardPoint(0, 2), new BoardPoint(1, 1), new BoardPoint(1, 2), new BoardPoint(2, 1) },
                        new[] { new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(2, 1), new BoardPoint(2, 2) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(2, 0) }
                    })
            },
            {
                TetrominoType.J,
                new Tetromino(
                    TetrominoType.J,
                    "j",
                    new[]
                    {
                        new[] { new BoardPoint(0, 0), new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(1, 2) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(0, 2), new BoardPoint(1, 1), new BoardPoint(2, 1) },
                        new[] { new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(1, 2), new BoardPoint(2, 2) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(1, 1), new BoardPoint(2, 0), new BoardPoint(2, 1) }
                    })
            },
            {
                TetrominoType.L,
                new Tetromino(
                    TetrominoType.L,
                    "l",
                    new[]
                    {
                        new[] { new BoardPoint(0, 2), new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(1, 2) },
                        new[] { new BoardPoint(0, 1), new BoardPoint(1, 1), new BoardPoint(2, 1), new BoardPoint(2, 2) },
                        new[] { new BoardPoint(1, 0), new BoardPoint(1, 1), new BoardPoint(1, 2), new BoardPoint(2, 0) },
                        new[] { new BoardPoint(0, 0), new BoardPoint(0, 1), new BoardPoint(1, 1), new BoardPoint(2, 1) }
                    })
            }
        };

        return new ReadOnlyDictionary<TetrominoType, Tetromino>(map);
    }

    private static IReadOnlyDictionary<(TetrominoType, int, int), BoardPoint[]> BuildKickTable()
    {
        var table = new Dictionary<(TetrominoType, int, int), BoardPoint[]>();

        static BoardPoint[] Offsets(params (int Row, int Col)[] points)
        {
            var result = new BoardPoint[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                result[i] = new BoardPoint(points[i].Row, points[i].Col);
            }
            return result;
        }

        var jlstz = new (int From, int To, BoardPoint[] Offsets)[]
        {
            (0, 1, Offsets((0, 0), (0, -1), (-1, -1), (2, 0), (2, -1))),
            (1, 0, Offsets((0, 0), (0, 1), (1, 1), (-2, 0), (-2, 1))),
            (1, 2, Offsets((0, 0), (0, 1), (1, 1), (-2, 0), (-2, 1))),
            (2, 1, Offsets((0, 0), (0, -1), (-1, -1), (2, 0), (2, -1))),
            (2, 3, Offsets((0, 0), (0, 1), (-1, 1), (2, 0), (2, 1))),
            (3, 2, Offsets((0, 0), (0, -1), (1, -1), (-2, 0), (-2, -1))),
            (3, 0, Offsets((0, 0), (0, -1), (1, -1), (-2, 0), (-2, -1))),
            (0, 3, Offsets((0, 0), (0, 1), (-1, 1), (2, 0), (2, 1)))
        };

        foreach (var type in new[] { TetrominoType.J, TetrominoType.L, TetrominoType.S, TetrominoType.T, TetrominoType.Z })
        {
            foreach (var entry in jlstz)
            {
                table[(type, entry.From, entry.To)] = entry.Offsets;
            }
        }

        var iPiece = new (int From, int To, BoardPoint[] Offsets)[]
        {
            (0, 1, Offsets((0, 0), (0, -2), (0, 1), (-1, -2), (2, 1))),
            (1, 0, Offsets((0, 0), (0, 2), (0, -1), (1, 2), (-2, -1))),
            (1, 2, Offsets((0, 0), (0, -1), (0, 2), (2, -1), (-1, 2))),
            (2, 1, Offsets((0, 0), (0, 1), (0, -2), (-2, 1), (1, -2))),
            (2, 3, Offsets((0, 0), (0, 2), (0, -1), (1, 2), (-2, -1))),
            (3, 2, Offsets((0, 0), (0, -2), (0, 1), (-1, -2), (2, 1))),
            (3, 0, Offsets((0, 0), (0, -1), (0, 2), (-2, -1), (1, 2))),
            (0, 3, Offsets((0, 0), (0, 1), (0, -2), (2, 1), (-1, -2)))
        };

        foreach (var entry in iPiece)
        {
            table[(TetrominoType.I, entry.From, entry.To)] = entry.Offsets;
        }

        return new ReadOnlyDictionary<(TetrominoType, int, int), BoardPoint[]>(table);
    }
}
