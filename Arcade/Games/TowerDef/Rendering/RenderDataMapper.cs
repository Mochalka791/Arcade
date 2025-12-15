using Arcade.Games.TowerDef.Enemies;
using Arcade.Games.TowerDef.Pathing;
using Arcade.Games.TowerDef.Towers;
using Arcade.Games.TowerDef.Combat;

namespace Arcade.Games.TowerDef.Rendering;

public static class RenderDataMapper
{
    public static RenderData Map(
        IEnumerable<PathPoint> path,
        IEnumerable<Enemy> enemies,
        IEnumerable<Tower> towers,
        IEnumerable<Projectile> projectiles)
    {
        return new RenderData
        {
            path = path.Select(p => new PathDto
            {
                x = p.X,
                y = p.Y
            }).ToList(),

            enemies = enemies.Select(e => new EnemyDto
            {
                x = e.X,
                y = e.Y,
                hp = e.HP,
                maxHP = e.MaxHP,
                size = GetEnemySize(e),
                type = (int)e.Type,
                isBoss = IsBoss(e),
                hasPoison = e.Effects.Any(x => x.Type == EnemyEffectType.Poison),
                hasSlow = e.Effects.Any(x => x.Type == EnemyEffectType.Slow)
            }).ToList(),

            towers = towers.Select(t => new TowerDto
            {
                x = t.X,
                y = t.Y,
                type = (int)t.Type,
                level = t.Level,
                range = t.GetRange()
            }).ToList(),

            projectiles = projectiles.Select(p => new ProjectileDto
            {
                x = p.X,
                y = p.Y,
                type = (int)p.SourceTower
            }).ToList()
        };
    }

    private static bool IsBoss(Enemy enemy)
    {
        return enemy.Type == EnemyType.BossBrute
            || enemy.Type == EnemyType.BossMage
            || enemy.Type == EnemyType.BossSummoner;
    }

    private static float GetEnemySize(Enemy enemy)
    {
        return enemy.Type switch
        {
            EnemyType.Fast => 12,
            EnemyType.Tank => 20,
            EnemyType.Summoner => 16,
            EnemyType.BossBrute => 30,
            EnemyType.BossMage => 28,
            EnemyType.BossSummoner => 28,
            _ => 15
        };
    }
}