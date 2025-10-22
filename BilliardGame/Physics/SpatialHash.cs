using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BilliardGame.Physics;

public sealed class SpatialHash
{
    private static readonly Point[] NeighborOffsets =
    {
        new Point(-1, -1),
        new Point(0, -1),
        new Point(1, -1),
        new Point(-1, 0),
        new Point(1, 0),
        new Point(-1, 1),
        new Point(0, 1),
        new Point(1, 1)
    };

    private readonly Dictionary<Point, List<Ball>> _cells = new();
    private readonly float _cellSize;

    public SpatialHash(float cellSize)
    {
        _cellSize = cellSize;
    }

    public void Clear() => _cells.Clear();

    public void Insert(Ball ball)
    {
        var key = GetCell(ball.Position);
        if (!_cells.TryGetValue(key, out var list))
        {
            list = new List<Ball>();
            _cells[key] = list;
        }

        list.Add(ball);
    }

    public IEnumerable<(Ball, Ball)> GetPairs()
    {
        foreach (var entry in _cells)
        {
            var key = entry.Key;
            var list = entry.Value;

            for (var i = 0; i < list.Count; i++)
            {
                for (var j = i + 1; j < list.Count; j++)
                {
                    yield return (list[i], list[j]);
                }
            }

            foreach (var offset in NeighborOffsets)
            {
                var neighborKey = new Point(key.X + offset.X, key.Y + offset.Y);
                if (!ShouldProcessNeighbor(key, neighborKey))
                {
                    continue;
                }

                if (_cells.TryGetValue(neighborKey, out var neighborList))
                {
                    foreach (var ball in list)
                    {
                        foreach (var other in neighborList)
                        {
                            yield return (ball, other);
                        }
                    }
                }
            }
        }
    }

    private Point GetCell(Vector2 position)
    {
        var x = (int)MathF.Floor(position.X / _cellSize);
        var y = (int)MathF.Floor(position.Y / _cellSize);
        return new Point(x, y);
    }

    private static bool ShouldProcessNeighbor(Point current, Point neighbor)
    {
        if (neighbor.Y > current.Y)
        {
            return true;
        }

        if (neighbor.Y == current.Y && neighbor.X > current.X)
        {
            return true;
        }

        return false;
    }
}
