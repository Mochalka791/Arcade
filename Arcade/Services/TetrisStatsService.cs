using Arcade.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arcade.Data.Services;

public class TetrisGameResultDto
{
    public int Score { get; set; }
}

public class TetrisTopDto
{
    public string UserName { get; set; } = "";
    public int HighScore { get; set; }
}

public interface ITetrisStatsService
{
    Task UpdateAfterGameAsync(
        int userId,
        int score,
        int level,
        bool isGameOver,
        CancellationToken cancellationToken = default);
}

public class TetrisStatsService : ITetrisStatsService
{
    private readonly ArcadeDbContext _db;
    private readonly ILogger<TetrisStatsService> _logger;

    public TetrisStatsService(ArcadeDbContext db, ILogger<TetrisStatsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task UpdateAfterGameAsync(
        int userId,
        int score,
        int level,
        bool isGameOver,
        CancellationToken cancellationToken = default)
    {
        var stats = await _db.TetrisStats
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (stats == null)
        {
            stats = new TetrisStats
            {
                UserId = userId,
                HighScore = 0,
                GamesPlayed = 0,
                MaxLevel = 0
            };
            _db.TetrisStats.Add(stats);
        }

        if (isGameOver)
        {
            stats.GamesPlayed++;
        }

        if (score > stats.HighScore)
            stats.HighScore = score;

        if (level > stats.MaxLevel)
            stats.MaxLevel = level;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "TetrisStats updated for User {UserId}: score={Score}, high={High}, games={Games}, maxLevel={MaxLevel}, isGameOver={IsGameOver}",
            userId, score, stats.HighScore, stats.GamesPlayed, stats.MaxLevel, isGameOver);
    }
}
