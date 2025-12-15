namespace Arcade.Games.TowerDefense.Core;

public class GameState
{
    public int Gold { get; set; } = 250;
    public int Lives { get; set; } = 20;

    public int CurrentWave { get; set; } = 0;
    public int NextWave { get; set; } = 1;

    public bool GameOver => Lives <= 0;

    public float GameSpeed { get; set; } = 1.0f;

    public int TotalGoldEarned { get; set; }
    public int TotalEnemiesKilled { get; set; }

    public void Reset()
    {
        Gold = 250;
        Lives = 20;
        CurrentWave = 0;
        NextWave = 1;
        GameSpeed = 1.0f;
        TotalGoldEarned = 0;
        TotalEnemiesKilled = 0;
    }
}