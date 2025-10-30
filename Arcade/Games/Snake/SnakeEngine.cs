using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcade.Games.Snake;

public sealed class SnakeEngine
{
    public const int Width = 20;
    public const int Height = 20;

    private readonly Random _rng = new();
    private (int x, int y) _head;
    private (int x, int y) _food;
    private int _foodsEaten;

    public HashSet<(int x, int y)> Body { get; } = new();
    private Queue<(int x, int y)> Order { get; set; } = new();

    public Direction Dir { get; private set; } = Direction.Right;
    public int Score { get; private set; }
    public int HighScore { get; set; }
    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool WrapWalls { get; set; }
    public TimeSpan TickInterval { get; private set; } = TimeSpan.FromMilliseconds(180);
    public bool AteFoodLastTick { get; private set; }

    public (int x, int y) Head => _head;
    public (int x, int y) Food => _food;

    public void Reset()
    {
        Order = new Queue<(int x, int y)>();
        Body.Clear();
        Score = 0;
        IsPaused = false;
        IsGameOver = false;
        Dir = Direction.Right;
        TickInterval = TimeSpan.FromMilliseconds(180);
        _foodsEaten = 0;
        AteFoodLastTick = false;

        var y = Height / 2;
        var x = Width / 2 - 1;
        var initial = new List<(int x, int y)> { (x - 2, y), (x - 1, y), (x, y) };
        foreach (var seg in initial)
        {
            Order.Enqueue(seg);
            Body.Add(seg);
        }
        _head = initial.Last();
        SpawnFood();
    }

    public bool Tick()
    {
        if (IsPaused || IsGameOver) return false;

        AteFoodLastTick = false;

        // WICHTIG: next als BENANNTES Tupel deklarieren
        (int x, int y) next = Dir switch
        {
            Direction.Up => (_head.x, _head.y - 1),
            Direction.Down => (_head.x, _head.y + 1),
            Direction.Left => (_head.x - 1, _head.y),
            _ => (_head.x + 1, _head.y),
        };

        if (WrapWalls)
        {
            // Optional: Namen beim Zuweisen explizit beibehalten
            next = (x: (next.x + Width) % Width, y: (next.y + Height) % Height);
        }
        else
        {
            if (next.x < 0 || next.y < 0 || next.x >= Width || next.y >= Height)
            {
                IsGameOver = true;
                return false;
            }
        }

        var eats = next == _food;
        var tail = Order.Peek();

        if (!eats)
        {
            Order.Dequeue();
            Body.Remove(tail);
        }

        if (Body.Contains(next))
        {
            IsGameOver = true;
            return false;
        }

        Order.Enqueue(next);
        Body.Add(next);
        _head = next;

        if (eats)
        {
            Score += 10;
            HighScore = Math.Max(HighScore, Score);
            _foodsEaten++;
            AteFoodLastTick = true;

            if (_foodsEaten % 5 == 0)
            {
                var ms = Math.Max(80, (int)TickInterval.TotalMilliseconds - 15);
                TickInterval = TimeSpan.FromMilliseconds(ms);
            }
            SpawnFood();
        }

        return true;
    }

    public void Pause() => IsPaused = true;
    public void Resume() { if (!IsGameOver) IsPaused = false; }
    public void TogglePause() { if (!IsGameOver) IsPaused = !IsPaused; }

    public void TurnLeft() { if (Dir != Direction.Right) Dir = Direction.Left; }
    public void TurnRight() { if (Dir != Direction.Left) Dir = Direction.Right; }
    public void TurnUp() { if (Dir != Direction.Down) Dir = Direction.Up; }
    public void TurnDown() { if (Dir != Direction.Up) Dir = Direction.Down; }

    private void SpawnFood()
    {
        var free = Width * Height - Body.Count;
        if (free <= 0) { IsGameOver = true; return; }

        (int x, int y) p;
        do { p = (_rng.Next(0, Width), _rng.Next(0, Height)); } while (Body.Contains(p));
        _food = p;
    }
}

public enum Direction { Up, Down, Left, Right }
