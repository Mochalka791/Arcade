using Arcade.Games.TowerDef.Enemies;
using Arcade.Games.TowerDef.Towers;

namespace Arcade.Games.TowerDef.Combat;

public static class DamageSystem
{
    public static void Apply(
        Projectile projectile,
        Enemy enemy,
        List<Enemy> allEnemies,
        List<Projectile> projectiles)
    {
        if (enemy.IsDead)
            return;

        enemy.HP -= projectile.Damage;

        // Status Effects
        switch (projectile.SourceTower)
        {
            case TowerType.Freeze:
                StatusEffects.ApplyFreeze(enemy, duration: 2f, slowAmount: 0.5f);
                break;

            case TowerType.Poison:
                StatusEffects.ApplyPoison(
                    enemy,
                    duration: 3f,
                    damagePerTick: projectile.Damage * 2);
                break;
        }

        // Chain Lightning
        if (projectile.SourceTower == TowerType.Lightning &&
            projectile.ChainLeft > 0)
        {
            var nextTarget = FindNextChainTarget(
                enemy,
                allEnemies);

            if (nextTarget != null)
            {
                projectiles.Add(new Projectile(
                    enemy.X,
                    enemy.Y,
                    nextTarget,
                    (int)(projectile.Damage * 0.7f),
                    projectile.SourceTower,
                    projectile.ChainLeft - 1
                ));
            }
        }
    }

    private static Enemy? FindNextChainTarget(
        Enemy from,
        List<Enemy> enemies)
    {
        return enemies
            .Where(e => !e.IsDead && e != from)
            .OrderBy(e => Distance(e.X, e.Y, from.X, from.Y))
            .FirstOrDefault(e => Distance(e.X, e.Y, from.X, from.Y) < 120);
    }

    private static float Distance(
        float x1, float y1,
        float x2, float y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}