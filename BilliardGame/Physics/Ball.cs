using Microsoft.Xna.Framework;

namespace BilliardGame.Physics;

public enum BallCategory
{
    Cue,
    Solid,
    Stripe,
    Eight
}

public sealed class Ball
{
    private const float SleepVelocityThreshold = 0.02f;
    private const float SleepAngularThreshold = 0.02f;

    public Ball(int number, BallCategory category, float radius, float mass, Color color)
    {
        Number = number;
        Category = category;
        Radius = radius;
        Mass = mass;
        Color = color;
    }

    public int Number { get; }
    public BallCategory Category { get; }
    public float Radius { get; }
    public float Mass { get; }
    public Color Color { get; }

    public Vector2 Position;
    public Vector2 Velocity;
    public float AngularVelocity;
    public bool InPocket;

    public float Diameter => Radius * 2f;
    public bool IsCueBall => Category == BallCategory.Cue;

    public bool Sleeping => Velocity.LengthSquared() <= SleepVelocityThreshold * SleepVelocityThreshold
        && MathF.Abs(AngularVelocity) <= SleepAngularThreshold;

    public void ApplyDamping(float dt, float rollFriction)
    {
        if (InPocket)
        {
            Stop();
            return;
        }

        var decay = MathF.Exp(-rollFriction * dt);
        Velocity *= decay;
        AngularVelocity *= decay;

        if (Velocity.LengthSquared() <= SleepVelocityThreshold * SleepVelocityThreshold)
        {
            Velocity = Vector2.Zero;
        }

        if (MathF.Abs(AngularVelocity) <= SleepAngularThreshold)
        {
            AngularVelocity = 0f;
        }
    }

    public void Stop()
    {
        Velocity = Vector2.Zero;
        AngularVelocity = 0f;
    }

    public void Reset(Vector2 position)
    {
        Position = position;
        InPocket = false;
        Stop();
    }
}
