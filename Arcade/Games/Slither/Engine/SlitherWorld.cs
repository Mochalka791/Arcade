using System;
using System.Collections.Generic;
using System.Linq;
using Arcade.Games.Slither.Models;

namespace Arcade.Games.Slither.Engine
{
    // Verwalten der Welt, Schlangen, Food und Kollisionen
    public class SlitherWorld
    {
        private readonly Random _rng = new();

        public float Width { get; } = 4000f;
        public float Height { get; } = 4000f;

        public List<SlitherSnake> Snakes { get; } = new();
        public List<SlitherFood> Food { get; } = new();

        public SlitherSnake Player => Snakes.First(s => s.Type == SnakeType.Player);

        // Initialisiert Spieler, Bots und Food
        public void Initialize(int botCount = 15, int foodCount = 500)
        {
            Snakes.Clear();
            Food.Clear();

            // Spieler
            var player = new SlitherSnake
            {
                Type = SnakeType.Player,
                Name = "Player",
                Color = "#5cf0c8",
                HeadPos = new Vec2(Width / 2f, Height / 2f),
                Direction = new Vec2(1, 0),
                Speed = 180f
            };
            InitSnakeSegments(player);
            Snakes.Add(player);

            // Bots
            for (int i = 0; i < botCount; i++)
            {
                var bot = new SlitherSnake
                {
                    Type = SnakeType.Bot,
                    Name = $"Bot {i + 1}",
                    Color = RandomColor(),
                    HeadPos = RandomPoint(),
                    Direction = new Vec2(1, 0),
                    Speed = 140f + (float)_rng.NextDouble() * 40f
                };
                InitSnakeSegments(bot);
                Snakes.Add(bot);
            }

            // Food
            for (int i = 0; i < foodCount; i++)
                SpawnFood();
        }

        // Startkörper erzeugen
        private void InitSnakeSegments(SlitherSnake snake)
        {
            snake.Segments.Clear();

            var spacing = snake.Radius * 2f;
            var dirBack = snake.Direction * -1;

            for (int i = 0; i < 20; i++)
            {
                var pos = snake.HeadPos + dirBack * (i * spacing);
                snake.Segments.Add(pos);
            }
        }

        private Vec2 RandomPoint()
        {
            return new Vec2(
                (float)_rng.NextDouble() * Width,
                (float)_rng.NextDouble() * Height
            );
        }

        private string RandomColor()
        {
            string[] colors =
            {
                "#5cf0c8", "#ff6b6b", "#ffd166",
                "#4dabf7", "#b197fc", "#ff922b"
            };
            return colors[_rng.Next(colors.Length)];
        }

        public void SpawnFood(Vec2? pos = null, float value = 1f)
        {
            var p = pos ?? RandomPoint();

            Food.Add(new SlitherFood
            {
                Id = Food.Count == 0 ? 1 : Food.Max(f => f.Id) + 1,
                Pos = p,
                Value = value,
                Color = value > 5f ? "#ff6b6b" : "#ffd166"
            });
        }

        // Haupt-Update: Input, Bewegung, Kollisionen
        public void Update(float dt, Vec2 inputDir)
        {
            if (Snakes.Count == 0) return;

            var player = Player;
            if (!player.IsDead)
                UpdatePlayer(player, inputDir, dt);

            foreach (var bot in Snakes.Where(s => s.Type == SnakeType.Bot && !s.IsDead))
                UpdateBot(bot, dt);

            foreach (var s in Snakes.Where(s => !s.IsDead))
                MoveSnake(s, dt);

            HandleFoodCollisions();
            HandleSnakeCollisions();
        }

        private void UpdatePlayer(SlitherSnake snake, Vec2 inputDir, float dt)
        {
            if (inputDir.Length < 0.01f)
                return;

            var target = inputDir.Normalized();
            var t = Math.Clamp(dt * 6f, 0f, 1f);

            snake.Direction = new Vec2(
                snake.Direction.X + (target.X - snake.Direction.X) * t,
                snake.Direction.Y + (target.Y - snake.Direction.Y) * t
            ).Normalized();
        }

        // sehr einfache Bot-Logik: zum nächsten Food
        private void UpdateBot(SlitherSnake snake, float dt)
        {
            var nearest = Food
                .OrderBy(f => (f.Pos - snake.HeadPos).Length)
                .FirstOrDefault();

            if (nearest == null)
                return;

            var target = (nearest.Pos - snake.HeadPos).Normalized();
            var t = Math.Clamp(dt * 4f, 0f, 1f);

            snake.Direction = new Vec2(
                snake.Direction.X + (target.X - snake.Direction.X) * t,
                snake.Direction.Y + (target.Y - snake.Direction.Y) * t
            ).Normalized();
        }

        private void MoveSnake(SlitherSnake s, float dt)
        {
            var head = s.HeadPos + s.Direction * (s.Speed * dt);

            head = new Vec2(
                Math.Clamp(head.X, 0f, Width),
                Math.Clamp(head.Y, 0f, Height)
            );

            s.HeadPos = head;

            var spacing = s.Radius * 2f;
            if (s.Segments.Count == 0)
                s.Segments.Add(head);
            else
                s.Segments[0] = head;

            for (int i = 1; i < s.Segments.Count; i++)
            {
                var prev = s.Segments[i - 1];
                var cur = s.Segments[i];
                var delta = prev - cur;
                var dist = delta.Length;

                if (dist > spacing)
                    s.Segments[i] = prev - delta.Normalized() * spacing;
            }
        }

        private void HandleFoodCollisions()
        {
            const float eatRadius = 14f;

            foreach (var s in Snakes.Where(s => !s.IsDead))
            {
                for (int i = Food.Count - 1; i >= 0; i--)
                {
                    var f = Food[i];
                    if ((f.Pos - s.HeadPos).Length <= eatRadius)
                    {
                        Food.RemoveAt(i);
                        GrowSnake(s, f.Value);
                        SpawnFood();
                    }
                }
            }
        }

        private void GrowSnake(SlitherSnake s, float value)
        {
            var spacing = s.Radius * 2f;
            var tail = s.Segments.Count > 0 ? s.Segments[^1] : s.HeadPos;
            int extra = (int)(value * 3);

            for (int i = 0; i < extra; i++)
                s.Segments.Add(tail);
        }

        private void HandleSnakeCollisions()
        {
            const float hitRadius = 10f;

            foreach (var s in Snakes.Where(s => !s.IsDead))
            {
                foreach (var other in Snakes.Where(o => !o.IsDead && !ReferenceEquals(o, s)))
                {
                    for (int i = 2; i < other.Segments.Count; i++)
                    {
                        var d = (s.HeadPos - other.Segments[i]).Length;
                        if (d < hitRadius)
                        {
                            KillSnake(s);
                            break;
                        }
                    }

                    if (s.IsDead)
                        break;
                }
            }
        }

        private void KillSnake(SlitherSnake s)
        {
            if (s.IsDead) return;

            s.IsDead = true;

            for (int i = 0; i < s.Segments.Count; i += 4)
                SpawnFood(s.Segments[i], value: 5f);
        }
    }
}
