namespace Arcade.Data.Entities;

public class SnakeStats
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int HighScore { get; set; }
    public int GamesPlayed { get; set; }
    public decimal AverageScore { get; set; }
}
