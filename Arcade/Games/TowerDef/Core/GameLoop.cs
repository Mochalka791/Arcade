namespace Arcade.Games.TowerDef.Core;

using Arcade.Games.TowerDef.Enemies;
using Arcade.Games.TowerDef.Towers;
using Arcade.Games.TowerDef.Combat;
using Arcade.Games.TowerDef.Pathing;
using Arcade.Games.TowerDefense.Core;
public class GameLoop
{
    private readonly GameState _state;
    private readonly List<PathPoint> _path;

    private readonly List<Enemy> _enemies;
    private readonly List<Tower> _towers;
    private readonly List<Projectile> _projectiles;

    public IReadOnlyList<Enemy> Enemies => _enemies;
    public IReadOnlyList<Tower> Towers => _towers;
    public IReadOnlyList<Projectile> Projectiles => _projectiles;

    public GameLoop(
        GameState state,
        List<PathPoint> path,
        List<Enemy> enemies,
        List<Tower> towers,
        List<Projectile> projectiles)
    {
        _state = state;
        _path = path;
        _enemies = enemies;
        _towers = towers;
        _projectiles = projectiles;
    }

    public void Tick(float deltaTime)
    {
        if (_state.GameOver)
            return;

        UpdateEnemies(deltaTime);
        UpdateTowers(deltaTime);
        UpdateProjectiles(deltaTime);
    }

    private void UpdateEnemies(float dt)
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];

            enemy.Update(dt);

            if (enemy.ReachedEnd)
            {
                _state.Lives--;
                _enemies.RemoveAt(i);
                continue;
            }

            if (enemy.IsDead)
            {
                _state.Gold += enemy.GoldReward;
                _state.TotalGoldEarned += enemy.GoldReward;
                _state.TotalEnemiesKilled++;
                _enemies.RemoveAt(i);
            }
        }
    }

    private void UpdateTowers(float dt)
    {
        foreach (var tower in _towers)
        {
            tower.Update(dt);

            var target = tower.FindTarget(_enemies);
            if (target == null)
                continue;

            var projectile = tower.Fire(target);
            if (projectile != null)
            {
                _projectiles.Add(projectile);
            }
        }
    }

    private void UpdateProjectiles(float dt)
    {
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = _projectiles[i];
            projectile.Update(dt);

            if (projectile.HasHit)
            {
                DamageSystem.Apply(projectile, projectile.Target,_enemies,_projectiles);
                _projectiles.RemoveAt(i);
            }
        }
    }
}