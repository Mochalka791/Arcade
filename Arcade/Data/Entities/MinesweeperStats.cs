public class MinesweeperStats
{
    public int Id { get; set; }

    public string UserId { get; set; } = default!;
    public string Difficulty { get; set; } = default!;

    public double TimeSeconds { get; set; }
    public int ThreeBV { get; set; }
    public double ThreeBVPerSecond { get; set; }
    public int Clicks { get; set; }
    public double EfficiencyPercent { get; set; }

    public bool Won { get; set; }

    public int Points { get; set; }  

    public DateTime PlayedAt { get; set; }
}
