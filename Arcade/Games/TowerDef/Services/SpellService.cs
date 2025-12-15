namespace Arcade.Games.TowerDef.Services;
using Arcade.Games.TowerDefense.Core;
using Arcade.Games.TowerDef.Enemies;
using Arcade.Games.TowerDef.Combat;


public class SpellService
{
    private readonly GameState _state;
    private readonly List<Enemy> _enemies;

    public SpellService(GameState state, List<Enemy> enemies)
    {
        _state = state;
        _enemies = enemies;
    }

    /// <summary>
    /// Beispiel‑Spell: Freeze All
    /// </summary>
    public void FreezeAll(float duration, float slowAmount)
    {
        if (_state.GameOver)
            return;

        foreach (var enemy in _enemies)
        {
            StatusEffects.ApplyFreeze(enemy, duration, slowAmount);
        }
    }

    /// <summary>
    /// Beispiel‑Spell: Global Damage
    /// </summary>
    public void DamageAll(int damage)
    {
        if (_state.GameOver)
            return;

        foreach (var enemy in _enemies)
        {
            enemy.HP -= damage;
        }
    }
}