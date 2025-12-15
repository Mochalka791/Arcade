namespace Arcade.Games.TowerDef.Towers;
using Arcade.Games.TowerDef.Enemies;
using Arcade.Games.TowerDef.Combat;


public class Tower
{
    public TowerType Type { get; }

    public float X { get; }
    public float Y { get; }

    public int Level { get; private set; } = 1;

    public float BaseRange { get; }
    public float BaseFireRate { get; }
    public int BaseDamage { get; }

    private float _cooldown;

    public Tower(
        TowerType type,
        float x,
        float y,
        float range,
        float fireRate,
        int damage)
    {
        Type = type;
        X = x;
        Y = y;
        BaseRange = range;
        BaseFireRate = fireRate;
        BaseDamage = damage;
    }

    public void Update(float dt)
    {
        if (_cooldown > 0)
            _cooldown -= dt;
    }

    public Enemy? FindTarget(IEnumerable<Enemy> enemies)
    {
        var range = GetRange();

        return enemies
            .Where(e => !e.IsDead &&
                        Distance(e.X, e.Y, X, Y) <= range)
            .OrderByDescending(e => e.PathIndex)
            .FirstOrDefault();
    }

    public Projectile? Fire(Enemy target)
    {
        if (_cooldown > 0)
            return null;

        _cooldown = GetFireRate();

        return new Projectile(
            X,
            Y,
            target,
            GetDamage(),
            Type,
            GetChainCount()
        );
    }

    public void Upgrade()
    {
        Level++;
    }

    public float GetRange()
        => BaseRange * (1f + (Level - 1) * 0.15f);

    public float GetFireRate()
        => BaseFireRate / (1f + (Level - 1) * 0.2f);

    public int GetDamage()
        => (int)(BaseDamage * (1f + (Level - 1) * 0.5f));

    private int GetChainCount()
    {
        if (Type == TowerType.Lightning && Level >= 2)
            return 2 + (Level - 2);

        return 0;
    }

    private static float Distance(float x1, float y1, float x2, float y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}