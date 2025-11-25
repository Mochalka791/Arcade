using Arcade.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arcade.Data.Services;

public class SnakeGameResultDto
{
    public int Score { get; set; }
}
public class SnakeTopDto
{
    public string UserName { get; set; } = "";
    public int HighScore { get; set; }
}
public interface ISnakeStatsService
{
    Task UpdateAfterGameAsync(int userId, int score, CancellationToken cancellationToken = default);
}

public class SnakeStatsService : ISnakeStatsService
{
    private readonly ArcadeDbContext _db;
    private readonly ILogger<SnakeStatsService> _logger;

    public SnakeStatsService(ArcadeDbContext db, ILogger<SnakeStatsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task UpdateAfterGameAsync(int userId, int score, CancellationToken cancellationToken = default)
    {
        var stats = await _db.SnakeStats
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (stats == null)
        {
            stats = new SnakeStats
            {
                UserId = userId,
                HighScore = score,
                GamesPlayed = 0,
                AverageScore = 0m
            };
            _db.SnakeStats.Add(stats);
        }

        stats.GamesPlayed++;

        if (score > stats.HighScore)
        {
            stats.HighScore = score;
        }

        stats.AverageScore =
            ((stats.AverageScore * (stats.GamesPlayed - 1)) + score)
            / stats.GamesPlayed;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "SnakeStats updated for User {UserId}: Score {Score}, GamesPlayed {GamesPlayed}, HighScore {HighScore}, Avg {AverageScore}",
            userId, score, stats.GamesPlayed, stats.HighScore, stats.AverageScore);
    }
}
