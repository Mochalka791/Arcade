namespace Arcade.Games.Minesweeper;

public static class MinesweeperStatsEngine
{
    public static int Calculate3BV(MinesweeperEngine engine)
        => engine.RevealedSafeCells;

    public static double Calculate3BVPerSecond(int threeBV, double seconds)
        => seconds > 0.0
            ? threeBV / seconds
            : 0.0;

    public static double CalculateEfficiency(int threeBV, int clicks)
        => clicks > 0
            ? (threeBV / (double)clicks) * 100.0
            : 0.0;

    public static int CalculatePoints(string difficulty, bool won)
    {
        if (!won)
        {
            return 0;
        }

        var rng = Random.Shared;

        return difficulty switch
        {
            "Beginner" => rng.Next(1, 4),  
            "Intermediate" => rng.Next(3, 8),   
            "Expert" => rng.Next(7, 16),  
            _ => 0
        };
    }
}
