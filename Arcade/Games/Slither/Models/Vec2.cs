using System;

namespace Arcade.Games.Slither.Models
{
    public readonly record struct Vec2(float X, float Y)
    {
        public static Vec2 Zero => new(0, 0);

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public Vec2 Normalized()
        {
            var len = Length;
            return len > 0.0001f ? new Vec2(X / len, Y / len) : Zero;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, float s) => new(a.X * s, a.Y * s);
    }
}
