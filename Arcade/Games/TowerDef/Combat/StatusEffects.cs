using Arcade.Games.TowerDef.Enemies;

namespace Arcade.Games.TowerDef.Combat;

public static class StatusEffects
{
    public static void ApplyFreeze(Enemy enemy, float duration, float slowAmount)
    {
        enemy.AddOrRefreshEffect(
            EnemyEffectType.Slow,
            duration,
            intensity: slowAmount
        );
    }

    public static void ApplyPoison(
        Enemy enemy,
        float duration,
        int damagePerTick)
    {
        enemy.AddOrRefreshEffect(
            EnemyEffectType.Poison,
            duration,
            intensity: 0,
            damagePerTick: damagePerTick
        );
    }
}