namespace Arcade.Games.TowerDef.Core;

using Arcade.Games.TowerDef.Enemies;
using Arcade.Games.TowerDef.Pathing;
using Arcade.Games.TowerDefense.Core; 

public class WaveSystem
{
    private readonly List<PathPoint> _path;
    private readonly EnemyFactory _enemyFactory;

    public int ActiveWaves { get; private set; }

    public WaveSystem(List<PathPoint> path, EnemyFactory enemyFactory)
    {
        _path = path;
        _enemyFactory = enemyFactory;
    }

    public async Task StartWaveAsync(
        int waveNumber,
        GameState state,
        List<Enemy> enemies,
        CancellationToken token)
    {
        ActiveWaves++;

        var config = GetWaveConfig(waveNumber);

        try
        {
            for (int i = 0; i < config.EnemyCount; i++)
            {
                if (token.IsCancellationRequested || state.GameOver)
                    break;

                var type = SelectEnemyType(waveNumber, i, config);
                var enemy = _enemyFactory.Create(type, waveNumber);

                enemies.Add(enemy);

                await Task.Delay(config.SpawnDelay, token);
            }
        }
        finally
        {
            ActiveWaves--;
        }
    }

    private WaveConfig GetWaveConfig(int wave)
    {
        return new WaveConfig
        {
            EnemyCount = 5 + wave * 3,
            SpawnDelay = Math.Max(200, 1000 - wave * 15),
            DifficultyMultiplier = 1 + wave * 0.15f
        };
    }

    private EnemyType SelectEnemyType(int wave, int index, WaveConfig config)
    {
        // Boss every 10 waves
        if (wave % 10 == 0 && index == config.EnemyCount - 1)
        {
            if (wave % 30 == 0) return EnemyType.BossSummoner;
            if (wave % 20 == 0) return EnemyType.BossMage;
            return EnemyType.BossBrute;
        }

        // Summoner chance
        if (wave > 5 && Random.Shared.Next(100) < wave * 2)
            return EnemyType.Summoner;

        var roll = Random.Shared.Next(100);
        if (roll < 30) return EnemyType.Fast;
        if (roll < 60) return EnemyType.Normal;
        return EnemyType.Tank;
    }

    private class WaveConfig
    {
        public int EnemyCount { get; init; }
        public int SpawnDelay { get; init; }
        public float DifficultyMultiplier { get; init; }
    }
}