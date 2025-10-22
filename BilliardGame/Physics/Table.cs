using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BilliardGame.Physics;

public sealed class Table
{
    private const float NearPocketRadiusMultiplier = 1.35f;

    public Table(float width, float height, float ballRadius)
    {
        Width = width;
        Height = height;
        BallRadius = ballRadius;
        PocketRadius = BallRadius * 2.2f;
        RailRestitution = 0.95f;
        BallRestitution = 0.95f;
        RollFriction = 0.6f;
        Pockets =
        [
            new Vector2(0f, 0f),
            new Vector2(width / 2f, 0f),
            new Vector2(width, 0f),
            new Vector2(0f, height),
            new Vector2(width / 2f, height),
            new Vector2(width, height)
        ];
    }

    public float Width { get; }
    public float Height { get; }
    public float BallRadius { get; }
    public float PocketRadius { get; }
    public float RailRestitution { get; }
    public float BallRestitution { get; }
    public float RollFriction { get; }
    public IReadOnlyList<Vector2> Pockets { get; }

    public static Table CreateStandard() => new(980f, 490f, 14f);

    public bool TryPocket(Ball ball)
    {
        foreach (var pocket in Pockets)
        {
            if (Vector2.DistanceSquared(ball.Position, pocket) <= PocketRadius * PocketRadius)
            {
                ball.InPocket = true;
                ball.Position = pocket;
                ball.Stop();
                return true;
            }
        }

        return false;
    }

    public void ResolveCushion(Ball ball)
    {
        if (ball.InPocket)
        {
            return;
        }

        if (IsNearPocket(ball))
        {
            return;
        }

        var minX = BallRadius;
        var maxX = Width - BallRadius;
        var minY = BallRadius;
        var maxY = Height - BallRadius;

        if (ball.Position.X < minX)
        {
            var penetration = minX - ball.Position.X;
            ball.Position.X = minX + penetration;
            ball.Velocity.X = MathF.Abs(ball.Velocity.X) * RailRestitution;
        }
        else if (ball.Position.X > maxX)
        {
            var penetration = ball.Position.X - maxX;
            ball.Position.X = maxX - penetration;
            ball.Velocity.X = -MathF.Abs(ball.Velocity.X) * RailRestitution;
        }

        if (ball.Position.Y < minY)
        {
            var penetration = minY - ball.Position.Y;
            ball.Position.Y = minY + penetration;
            ball.Velocity.Y = MathF.Abs(ball.Velocity.Y) * RailRestitution;
        }
        else if (ball.Position.Y > maxY)
        {
            var penetration = ball.Position.Y - maxY;
            ball.Position.Y = maxY - penetration;
            ball.Velocity.Y = -MathF.Abs(ball.Velocity.Y) * RailRestitution;
        }
    }

    private bool IsNearPocket(Ball ball)
    {
        var threshold = (PocketRadius + ball.Radius) * NearPocketRadiusMultiplier;
        var thresholdSquared = threshold * threshold;
        foreach (var pocket in Pockets)
        {
            if (Vector2.DistanceSquared(ball.Position, pocket) <= thresholdSquared)
            {
                return true;
            }
        }

        return false;
    }
}
