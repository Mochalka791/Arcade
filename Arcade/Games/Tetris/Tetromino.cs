using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Arcade.Games.Tetris;

public enum TetrominoType { I, O, T, S, Z, J, L }

public readonly record struct BoardPoint(int Row, int Col)
{
    public static BoardPoint operator +(BoardPoint left, BoardPoint right)
        => new(left.Row + right.Row, left.Col + right.Col);
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

    public BoardPoint[] GetRotation(int rotation)
        => Rotations[NormalizeRotation(rotation)];

    public static int NormalizeRotation(int rotation)
    {
        var n = rotation % 4;
        return n < 0 ? n + 4 : n;
    }
}

public static class Tetrominoes
{
    private static readonly IReadOnlyDictionary<TetrominoType, Tetromino> _tetrominoes = BuildTetrominoes();

    public static Tetromino Get(TetrominoType type) => _tetrominoes[type];

    private static IReadOnlyDictionary<TetrominoType, Tetromino> BuildTetrominoes()
    {
        var map = new Dictionary<TetrominoType, Tetromino>
        {
            {
                TetrominoType.I,
                new Tetromino(
                    TetrominoType.I, "i",
                    new[]
                    {
                        new[] { new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(1,2), new BoardPoint(1,3) }, // 0°
                        new[] { new BoardPoint(0,2), new BoardPoint(1,2), new BoardPoint(2,2), new BoardPoint(3,2) }, // 90°
                        new[] { new BoardPoint(2,0), new BoardPoint(2,1), new BoardPoint(2,2), new BoardPoint(2,3) }, // 180°
                        new[] { new BoardPoint(0,1), new BoardPoint(1,1), new BoardPoint(2,1), new BoardPoint(3,1) }  // 270°
                    })
            },
            {
                TetrominoType.O,
                new Tetromino(
                    TetrominoType.O, "o",
                    new[]
                    {
                        new[] { new BoardPoint(0,1), new BoardPoint(0,2), new BoardPoint(1,1), new BoardPoint(1,2) },
                        new[] { new BoardPoint(0,1), new BoardPoint(0,2), new BoardPoint(1,1), new BoardPoint(1,2) },
                        new[] { new BoardPoint(0,1), new BoardPoint(0,2), new BoardPoint(1,1), new BoardPoint(1,2) },
                        new[] { new BoardPoint(0,1), new BoardPoint(0,2), new BoardPoint(1,1), new BoardPoint(1,2) }
                    })
            },
            {
                TetrominoType.T,
                new Tetromino(
                    TetrominoType.T, "t",
                    new[]
                    {
                        new[] { new BoardPoint(0,1), new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(1,2) },
                        new[] { new BoardPoint(0,1), new BoardPoint(1,1), new BoardPoint(1,2), new BoardPoint(2,1) },
                        new[] { new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(1,2), new BoardPoint(2,1) },
                        new[] { new BoardPoint(0,1), new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(2,1) }
                    })
            },
            {
                TetrominoType.S,
                new Tetromino(
                    TetrominoType.S, "s",
                    new[]
                    {
                        new[] { new BoardPoint(0,1), new BoardPoint(0,2), new BoardPoint(1,0), new BoardPoint(1,1) },
                        new[] { new BoardPoint(0,1), new BoardPoint(1,1), new BoardPoint(1,2), new BoardPoint(2,2) },
                        new[] { new BoardPoint(1,1), new BoardPoint(1,2), new BoardPoint(2,0), new BoardPoint(2,1) },
                        new[] { new BoardPoint(0,0), new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(2,1) }
                    })
            },
            {
                TetrominoType.Z,
                new Tetromino(
                    TetrominoType.Z, "z",
                    new[]
                    {
                        new[] { new BoardPoint(0,0), new BoardPoint(0,1), new BoardPoint(1,1), new BoardPoint(1,2) },
                        new[] { new BoardPoint(0,2), new BoardPoint(1,1), new BoardPoint(1,2), new BoardPoint(2,1) },
                        new[] { new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(2,1), new BoardPoint(2,2) },
                        new[] { new BoardPoint(0,1), new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(2,0) }
                    })
            },
            {
                TetrominoType.J,
                new Tetromino(
                    TetrominoType.J, "j",
                    new[]
                    {
                        new[] { new BoardPoint(0,0), new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(1,2) },
                        new[] { new BoardPoint(0,1), new BoardPoint(0,2), new BoardPoint(1,1), new BoardPoint(2,1) },
                        new[] { new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(1,2), new BoardPoint(2,2) },
                        new[] { new BoardPoint(0,1), new BoardPoint(1,1), new BoardPoint(2,0), new BoardPoint(2,1) }
                    })
            },
            {
                TetrominoType.L,
                new Tetromino(
                    TetrominoType.L, "l",
                    new[]
                    {
                        new[] { new BoardPoint(0,2), new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(1,2) },
                        new[] { new BoardPoint(0,1), new BoardPoint(1,1), new BoardPoint(2,1), new BoardPoint(2,2) },
                        new[] { new BoardPoint(1,0), new BoardPoint(1,1), new BoardPoint(1,2), new BoardPoint(2,0) },
                        new[] { new BoardPoint(0,0), new BoardPoint(0,1), new BoardPoint(1,1), new BoardPoint(2,1) }
                    })
            }
        };

        return new ReadOnlyDictionary<TetrominoType, Tetromino>(map);
    }
}
