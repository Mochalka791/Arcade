using Arcade.Games.TowerDef.Enemies;
using Arcade.Games.TowerDef.Towers;

namespace Arcade.Games.TowerDef.Combat;

public class Projectile
{
    public float X { get; private set; }
    public float Y { get; private set; }

    public Enemy Target { get; }
    public int Damage { get; }
    public TowerType SourceTower { get; }

    public int ChainLeft { get; }

    public bool HasHit { get; private set; }

    private const float Speed = 300f;

    public Projectile(
        float x,
        float y,
        Enemy target,
        int damage,
        TowerType sourceTower,
        int chainLeft = 0)
    {
        X = x;
        Y = y;
        Target = target;
        Damage = damage;
        SourceTower = sourceTower;
        ChainLeft = chainLeft;
    }

    public void Update(float dt)
    {
        if (HasHit || Target.IsDead)
        {
            HasHit = true;
            return;
        }

        var dx = Target.X - X;
        var dy = Target.Y - Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);

        if (dist < 8)
        {
            HasHit = true;
            return;
        }

        X += (dx / dist) * Speed * dt;
        Y += (dy / dist) * Speed * dt;
    }
}