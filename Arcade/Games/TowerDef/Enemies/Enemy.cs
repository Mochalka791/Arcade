namespace Arcade.Games.TowerDef.Enemies;

using Arcade.Games.TowerDef.Pathing;


public class Enemy
{
    private readonly List<PathPoint> _path;

    public EnemyType Type { get; }
    public int PathIndex { get; private set; }

    public float X { get; private set; }
    public float Y { get; private set; }

    public int HP { get; set; }
    public int MaxHP { get; }

    public float BaseSpeed { get; }
    public float SpeedMultiplier { get; set; } = 1f;

    public bool IsDead => HP <= 0;
    public bool ReachedEnd => PathIndex >= _path.Count - 1;

    public int GoldReward { get; }

    public List<EnemyEffect> Effects { get; } = new();

    // Summoner
    public float SummonCooldown { get; set; }
    public int SummonsLeft { get; set; }

    public Enemy(
        EnemyType type,
        int hp,
        float speed,
        int goldReward,
        List<PathPoint> path)
    {
        Type = type;
        HP = hp;
        MaxHP = hp;
        BaseSpeed = speed;
        GoldReward = goldReward;

        _path = path;
        X = path[0].X;
        Y = path[0].Y;
    }

    public void Update(float dt)
    {
        UpdateEffects(dt);
        Move(dt);
    }

    private void Move(float dt)
    {
        if (ReachedEnd)
            return;

        var next = _path[PathIndex + 1];
        var dx = next.X - X;
        var dy = next.Y - Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);

        if (dist < 4)
        {
            PathIndex++;
            return;
        }

        var speed = BaseSpeed * SpeedMultiplier;
        X += (dx / dist) * speed * dt;
        Y += (dy / dist) * speed * dt;
    }
    public void AddOrRefreshEffect(
    EnemyEffectType type,
    float duration,
    float intensity,
    int damagePerTick = 0)
    {
        var existing = Effects.FirstOrDefault(e => e.Type == type);

        if (existing != null)
        {
            existing.Duration = Math.Max(existing.Duration, duration);
        }
        else
        {
            Effects.Add(new EnemyEffect
            {
                Type = type,
                Duration = duration,
                Intensity = intensity,
                DamagePerTick = damagePerTick
            });
        }
    }
    private void UpdateEffects(float dt)
    {
        SpeedMultiplier = 1f;

        for (int i = Effects.Count - 1; i >= 0; i--)
        {
            var e = Effects[i];
            e.Duration -= dt;

            if (e.Duration <= 0)
            {
                Effects.RemoveAt(i);
                continue;
            }

            switch (e.Type)
            {
                case EnemyEffectType.Slow:
                    SpeedMultiplier = Math.Min(SpeedMultiplier, 1f - e.Intensity);
                    break;

                case EnemyEffectType.Poison:
                    e.TickTimer -= dt;
                    if (e.TickTimer <= 0)
                    {
                        HP -= e.DamagePerTick;
                        e.TickTimer = 0.5f;
                    }
                    break;
            }
        }
    }
}