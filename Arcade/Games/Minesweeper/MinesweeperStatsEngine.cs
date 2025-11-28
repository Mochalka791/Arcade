public static class MinesweeperStatsEngine
{
    public static int Calculate3BV(MinesweeperEngine e)
        => e.RevealedSafeCells;

    public static double Calculate3BVPerSecond(int threeBV, double seconds)
        => seconds > 0 ? threeBV / seconds : 0;

    public static double CalculateEfficiency(int threeBV, int clicks)
        => clicks > 0 ? (threeBV / (double)clicks) * 100.0 : 0;

    //public static string GetRank(bool win)
    //    => win ? "Rookie" : "Unranked";

    //public static int GetXP(bool win)
    //    => win ? 12 : 0;

    //public static int GetCoins(bool win)
    //    => win ? 1 : 0;

    //public static int GetTrophies(bool win)
    //    => win ? 8 : 0;
}
