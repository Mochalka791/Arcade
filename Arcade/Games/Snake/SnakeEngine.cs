using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcade.Games.Snake;

public class SnakeEngine
{
    public const int Width = 20;
    public const int Height = 20;

    private readonly Random _random = new();
    private (int x, int y) _foodPosition;
    private (int x, int y) _head;
    private int _foodsEaten;

    public Queue<(int x, int y)> Snake { get; private set; } = new();
    public Direction Dir { get; private set; } = Direction.Right;
    public HashSet<(int x, int y)> Body { get; } = new();
    public int Score { get; private set; }
    public int HighScore { get; set; }
    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool WrapWalls { get; set; }
    public TimeSpan TickInterval { get; private set; } = TimeSpan.FromMilliseconds(180);
    public bool AteFoodLastTick { get; private set; }

    public (int x, int y) FoodPosition => _foodPosition;
    public (int x, int y) Head => _head;

    public void Reset()
    {
        Snake = new Queue<(int x, int y)>();
        Body.Clear();
        Score = 0;
        IsPaused = false;
        IsGameOver = false;
        TickInterval = TimeSpan.FromMilliseconds(180);
        _foodsEaten = 0;
        AteFoodLastTick = false;
        Dir = Direction.Right;

        var startY = Height / 2;
        var startX = Width / 2 - 1;

        var initialSegments = new List<(int x, int y)>
        {
            (startX - 2, startY),
            (startX - 1, startY),
            (startX, startY)
        };

        foreach (var segment in initialSegments)
        {
            Snake.Enqueue(segment);
            Body.Add(segment);
        }

        _head = initialSegments.Last();

        SpawnFood();
    }

    public bool Tick()
    {
        if (IsPaused || IsGameOver)
        {
            return false;
        }

        AteFoodLastTick = false;
        var head = _head;
        var next = head;
        switch (Dir)
        {
            case Direction.Up:
                next = (head.x, head.y - 1);
                break;
            case Direction.Down:
                next = (head.x, head.y + 1);
                break;
            case Direction.Left:
                next = (head.x - 1, head.y);
                break;
            case Direction.Right:
                next = (head.x + 1, head.y);
                break;
        }

        if (WrapWalls)
        {
            next = ((next.x + Width) % Width, (next.y + Height) % Height);
        }
        else
        {
            if (next.x < 0 || next.y < 0 || next.x >= Width || next.y >= Height)
            {
                IsGameOver = true;
                return false;
            }
        }

        var ateFood = next == _foodPosition;
        var tail = Snake.Peek();

        if (!ateFood)
        {
            Snake.Dequeue();
            Body.Remove(tail);
        }

        if (Body.Contains(next))
        {
            IsGameOver = true;
            return false;
        }

        Snake.Enqueue(next);
        Body.Add(next);
        _head = next;

        if (ateFood)
        {
            Score += 10;
            if (Score > HighScore)
            {
                HighScore = Score;
            }

            _foodsEaten++;
            UpdateSpeed();
            SpawnFood();
            AteFoodLastTick = true;
        }

        return true;
    }

    public void Pause() => IsPaused = true;

    public void Resume()
    {
        if (!IsGameOver)
        {
            IsPaused = false;
        }
    }

    public void TogglePause()
    {
        if (IsGameOver)
        {
            return;
        }

        IsPaused = !IsPaused;
    }

    public void TurnLeft()
    {
        if (Dir != Direction.Right)
        {
            Dir = Direction.Left;
        }
    }

    public void TurnRight()
    {
        if (Dir != Direction.Left)
        {
            Dir = Direction.Right;
        }
    }

    public void TurnUp()
    {
        if (Dir != Direction.Down)
        {
            Dir = Direction.Up;
        }
    }

    public void TurnDown()
    {
        if (Dir != Direction.Up)
        {
            Dir = Direction.Down;
        }
    }

    public void SpawnFood()
    {
        var emptyCells = Width * Height - Body.Count;
        if (emptyCells <= 0)
        {
            IsGameOver = true;
            return;
        }

        (int x, int y) candidate;
        do
        {
            candidate = (_random.Next(0, Width), _random.Next(0, Height));
        } while (Body.Contains(candidate));

        _foodPosition = candidate;
    }

    public bool HasFoodAt(int x, int y) => _foodPosition.x == x && _foodPosition.y == y;

    private void UpdateSpeed()
    {
        if (_foodsEaten % 5 == 0)
        {
            var nextMs = Math.Max(80, (int)TickInterval.TotalMilliseconds - 15);
            TickInterval = TimeSpan.FromMilliseconds(nextMs);
        }
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
