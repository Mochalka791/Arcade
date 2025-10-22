using System.Collections.Generic;
using System.Linq;
using BilliardGame.Rules;
using Microsoft.Xna.Framework;

namespace BilliardGame.Physics;

public sealed class CollisionResolver
{
    private readonly Table _table;
    private readonly int _iterations;

    public CollisionResolver(Table table, int iterations = 4)
    {
        _table = table;
        _iterations = iterations;
    }

    public void Step(IList<Ball> balls, SpatialHash hash, GameRules rules)
    {
        hash.Clear();

        foreach (var ball in balls)
        {
            if (!ball.InPocket)
            {
                hash.Insert(ball);
            }
        }

        var pairs = hash.GetPairs().ToArray();

        for (var i = 0; i < _iterations; i++)
        {
            foreach (var (a, b) in pairs)
            {
                ResolvePair(a, b, rules);
            }
        }

        foreach (var ball in balls)
        {
            if (ball.InPocket)
            {
                continue;
            }

            _table.ResolveCushion(ball);
        }
    }

    private void ResolvePair(Ball a, Ball b, GameRules rules)
    {
        if (a.InPocket || b.InPocket)
        {
            return;
        }

        var delta = b.Position - a.Position;
        var distanceSquared = delta.LengthSquared();
        var combinedRadius = a.Radius + b.Radius;
        var combinedRadiusSquared = combinedRadius * combinedRadius;

        if (distanceSquared >= combinedRadiusSquared || distanceSquared <= 0f)
        {
            return;
        }

        var distance = MathF.Sqrt(distanceSquared);
        var normal = delta / distance;
        var penetration = combinedRadius - distance;

        a.Position -= normal * (penetration * 0.5f);
        b.Position += normal * (penetration * 0.5f);

        var relativeVelocity = Vector2.Dot(b.Velocity - a.Velocity, normal);
        if (relativeVelocity > 0f)
        {
            // Bouncing apart already
            rules.RegisterCollision(a, b);
            return;
        }

        var restitution = _table.BallRestitution;
        var impulseMagnitude = -(1f + restitution) * relativeVelocity / (1f / a.Mass + 1f / b.Mass);
        var impulse = normal * impulseMagnitude;

        a.Velocity -= impulse / a.Mass;
        b.Velocity += impulse / b.Mass;

        rules.RegisterCollision(a, b);
    }
}
