namespace Arcade.Games.TowerDef.Enemies;

public enum EnemyEffectType
{
    Slow,
    Poison
}

public class EnemyEffect
{
    public EnemyEffectType Type { get; init; }
    public float Duration { get; set; }
    public float Intensity { get; init; }
    public int DamagePerTick { get; init; }
    public float TickTimer { get; set; } = 0.5f;
}