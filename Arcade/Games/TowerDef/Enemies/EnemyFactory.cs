namespace Arcade.Games.TowerDef.Enemies;

using Arcade.Games.TowerDef.Pathing;

public class EnemyFactory
{
    private readonly PathSelector _pathSelector;
    public EnemyFactory(PathSelector pathSelector)
    {
        _pathSelector = pathSelector;
    }

    public Enemy Create(EnemyType type, int wave)
    {
        var diff = 1 + wave * 0.15f;

        var path = type is EnemyType.BossBrute
                           or EnemyType.BossMage
                           or EnemyType.BossSummoner
            ? _pathSelector.SelectBossPath()
            : _pathSelector.SelectPath();

        return type switch
        {
            EnemyType.Normal =>
                new Enemy(type, (int)(100 * diff), 50, 10 + wave * 2, path),

            EnemyType.Fast =>
                new Enemy(type, (int)(60 * diff), 80, 8 + wave * 2, path),

            EnemyType.Tank =>
                new Enemy(type, (int)(300 * diff), 30, 20 + wave * 3, path),

            EnemyType.Summoner =>
                new Enemy(type, (int)(150 * diff), 40, 25 + wave * 3, path)
                {
                    SummonCooldown = 5f,
                    SummonsLeft = 3
                },

            EnemyType.BossBrute =>
                new Enemy(type, (int)(1000 * diff), 25, 100 + wave * 10, path),

            EnemyType.BossMage =>
                new Enemy(type, (int)(800 * diff), 35, 120 + wave * 10, path),

            EnemyType.BossSummoner =>
                new Enemy(type, (int)(900 * diff), 30, 150 + wave * 10, path)
                {
                    SummonCooldown = 3f,
                    SummonsLeft = 6
                },

            _ => throw new ArgumentOutOfRangeException()
        };
    }
}